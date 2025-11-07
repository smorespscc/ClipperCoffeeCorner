using Azure.Communication.Sms;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Twilio.AspNet.Core;
using Twilio.Clients;
using Twilio.Types;
using Twilio.Rest.Api.V2010.Account;
using Twilio;
using WaitTimeTesting.Models;
using WaitTimeTesting.Options;

namespace WaitTimeTesting.Services
{
    public class WaitTimeNotificationService
    {
        private readonly ILogger<WaitTimeNotificationService> _logger;
        private readonly MLContext _mlContext = new MLContext(seed: 0);
        private ITransformer? _mlModel;  // Loaded ML model (nullable to satisfy CS8618)
        private readonly string _modelPath = "model.zip";  // In project root
        // private readonly SmsClient _smsClient; // Azure Communication Services SMS client
        private readonly TwilioRestClient? _twilioClient; // Twilio SMS client
        private readonly string _fromPhoneNumber;

        // In-memory "tables" for testing (simulate DB)
        private List<Order> ActiveOrders { get; } = new();
        private List<Order> CompletedPendingTraining { get; } = new();
        private List<Order> TrainedOrders { get; } = new();  // Optional archive

        // Retrain threshold (e.g., every 100 for testing; adjust)
        private const int RetrainThreshold = 100;

        public WaitTimeNotificationService(
            ILogger<WaitTimeNotificationService> logger,
            //SmsClient smsClient,                   // ACS stuff
            //IOptions<AzureSmsOptions> options)
            IOptions<TwilioSMSOptions> twilioOptions)   // Twilio stuff
        {
            _logger = logger;
            // _smsClient = smsClient;               // ACS stuff
            //_fromPhoneNumber = options.Value.FromPhoneNumber;
            _fromPhoneNumber = twilioOptions.Value.FromPhoneNumber;
            LoadOrTrainInitialModel();  // Setup initial ML model with simulated data or load existing
        }

        // Entry: Add order to queue
        public void AddOrder(Order order)
        {
            // check for and prevent duplicates
            if (ActiveOrders.Any(o => o.Uid == order.Uid))
            {
                _logger.LogError($"Duplicate order UID: {order.Uid}");
                throw new InvalidOperationException("Duplicate order UID.");
            }
            ActiveOrders.Add(order);
            _logger.LogInformation($"Added order {order.Uid} to queue. Current queue length: {ActiveOrders.Count}");
        }

        // Entry: Remove order from queue (when order is completed)
        public Order RemoveOrder(Guid uid)
        {
            var order = ActiveOrders.FirstOrDefault(o => o.Uid == uid);
            if (order == null)
            {
                _logger.LogError($"Order not found: {uid}");
                throw new KeyNotFoundException("Order not found in queue.");
            }
            ActiveOrders.Remove(order);
            _logger.LogInformation($"Removed order {uid} from queue. Current queue length: {ActiveOrders.Count}");
            return order;
        }

        // Get current queue length
        public int GetCurrentQueueLength() => ActiveOrders.Count;

        // Estimate wait time. Also prepares features for ML prediction
        public (double WaitTime, OrderData Features) EstimateWaitTime(Order order)
        {
            // Parse items
            var items = ParseItems(order.ItemIds);
            int itemCount = items.Count;
            int queueLength = ActiveOrders.Count;  // Includes this order. Could also set PlaceInQueue here

            var features = PrepareFeatures(order, queueLength, itemCount);
            double estimatedMinutes = PredictWithML(features);

            double waitTime = estimatedMinutes;
            _logger.LogInformation($"Estimated wait for {order.Uid}: {waitTime} min");
            return (waitTime, features);
        }

        // SMS notification with Azure Communication Services
        public async Task SendNotificationAsync(Order order, NotificationType type)
        {
            if (order.NotificationPref != NotificationPreference.Sms ||
                string.IsNullOrEmpty(order.PhoneNumber))
            {
                _logger.LogWarning($"Skipping SMS for {order.Uid}: Notif pref set to 0 or there is no phone number.");
                return;
            }

            // Currently sends item IDs, needs to query item names from DB or have them stored locally ig.
            string message = type switch
            {
                NotificationType.Placement =>
                    $"Order placed! Items: {order.ItemIds}\nPosition: {GetQueuePosition(order)}\nEst. wait: {(int)(order.EstimatedWaitTime ?? 0)} min\nWe'll let you know when your order is ready :)",
                NotificationType.Completion =>
                    $"Your order {order.Uid} is ready to be picked up!\n Your Items: {order.ItemIds}", // sending UID for testing but customer probably doesn't need to know it
                _ => "Update from Clipper Coffee Corner"
            };

            try
            {
                var messageResource = await MessageResource.CreateAsync(
                        body: message,
                        //to: new PhoneNumber("+18777804236"),         // Twilio virtual phone number for testing
                        to: new PhoneNumber(order.PhoneNumber),   // actual customer number
                        from: new PhoneNumber(_fromPhoneNumber)
                    );

                _logger.LogInformation($"[TWILIO SMS SENT] To: {order.PhoneNumber} | SID: {messageResource.Sid} | Status: {messageResource.Status}");


                //var response = await _smsClient.SendAsync(
                //    from: _fromPhoneNumber,
                //    to: order.PhoneNumber,
                //    message: message,
                //    options: new SmsSendOptions(enableDeliveryReport: true)
                //    {
                //        Tag = type == NotificationType.Placement ? "order-placed" : "order-complete"
                //    });
                //
                //var result = response.Value;
                //
                //if (result.Successful)
                //    _logger.LogInformation($"[SMS SENT] To: {order.PhoneNumber} | ID: {result.MessageId}");
                //else
                //    _logger.LogError($"[SMS FAILED] To: {order.PhoneNumber} | Error: {result.ErrorMessage}");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[SMS EXCEPTION] Failed to send to {order.PhoneNumber}");
            }
        }

        // Post-completion: Evaluate and log for retrain
        public void EvaluateAndLogForRetraining(Order completedOrder)
        {
            if (!completedOrder.CompletedAt.HasValue || !completedOrder.EstimatedWaitTime.HasValue)
            {
                _logger.LogWarning($"Cannot evaluate {completedOrder.Uid}: Missing times.");
                return;
            }

            // evaluate prediction accuracy
            double actualWait = (completedOrder.CompletedAt.Value - completedOrder.PlacedAt).TotalMinutes;
            double error = Math.Abs(actualWait - completedOrder.EstimatedWaitTime.Value);
            // Doesn't currently do anything with accuracy evaluation
            _logger.LogInformation($"Evaluation for {completedOrder.Uid}: Actual wait {actualWait:F2} min, Estimated {completedOrder.EstimatedWaitTime:F2} min, Error {error} min.");

            // add order to pending retrain list
            CompletedPendingTraining.Add(completedOrder);

            // trigger retrain if threshold met
            if (CompletedPendingTraining.Count >= RetrainThreshold)
            {
                RetrainModel();
            }
        }

        // Retrain ML model with pending data
        public void RetrainModel()
        {
            if (CompletedPendingTraining.Count == 0)
            {
                _logger.LogInformation("No new data for retraining.");
                return;
            }

            // Prep data: Convert to ML format
            var data = CompletedPendingTraining.Select(o =>
            {
                var items = ParseItems(o.ItemIds);
                int queueLength = o.PlaceInQueue;  // place in queue at time placed
                return new OrderData
                {
                    ItemCount = items.Count,
                    QueueLength = queueLength,
                    HourOfDay = o.PlacedAt.Hour,
                    DayOfWeek = (int)o.PlacedAt.DayOfWeek,
                    ItemsAhead = o.ItemsAheadAtPlacement,
                    TotalItemsAhead = o.TotalItemsAheadAtPlacement,
                    WaitMinutes = (float)(o.CompletedAt.Value - o.PlacedAt).TotalMinutes  // Label
                };
            }).ToList();

            IDataView trainingData = _mlContext.Data.LoadFromEnumerable(data);

            // Build pipeline
            var pipeline = _mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: nameof(OrderData.WaitMinutes))
                .Append(_mlContext.Transforms.Concatenate("Features", 
                    nameof(OrderData.ItemCount), 
                    nameof(OrderData.QueueLength), 
                    nameof(OrderData.HourOfDay), 
                    nameof(OrderData.DayOfWeek), 
                    nameof(OrderData.TotalItemsAhead),
                    nameof(OrderData.ItemsAhead)))
                .Append(_mlContext.Regression.Trainers.FastTree(labelColumnName: "Label", featureColumnName: "Features"));

            // Train
            _mlModel = pipeline.Fit(trainingData);

            // Evaluate (simple, on same data for testing; use split in real)
            var predictions = _mlModel.Transform(trainingData);
            var metrics = _mlContext.Regression.Evaluate(predictions, "Label");
            _logger.LogInformation($"Retrained model. RSquared: {metrics.RSquared}, RMS: {metrics.RootMeanSquaredError}");

            // Save
            _mlContext.Model.Save(_mlModel, trainingData.Schema, _modelPath);

            // Move to archive and clear orders pending training
            TrainedOrders.AddRange(CompletedPendingTraining);
            CompletedPendingTraining.Clear();
        }

        // Initial model setup with simulated data
        private void LoadOrTrainInitialModel()
        {
            if (File.Exists(_modelPath))
            {
                _mlModel = _mlContext.Model.Load(_modelPath, out var _);
                _logger.LogInformation("Loaded existing ML model.");
                return;
            }

            // Simulate 50 fake historical orders for initial training
            var simulatedData = GenerateSimulatedData(50);
            IDataView trainingData = _mlContext.Data.LoadFromEnumerable(simulatedData);

            // Same pipeline as retrain
            var pipeline = _mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: nameof(OrderData.WaitMinutes))
                .Append(_mlContext.Transforms.Concatenate("Features",
                    nameof(OrderData.ItemCount),
                    nameof(OrderData.QueueLength),
                    nameof(OrderData.HourOfDay),
                    nameof(OrderData.DayOfWeek),
                    nameof(OrderData.TotalItemsAhead),
                    nameof(OrderData.ItemsAhead)))
                .Append(_mlContext.Regression.Trainers.FastTree());

            _mlModel = pipeline.Fit(trainingData);
            _mlContext.Model.Save(_mlModel, trainingData.Schema, _modelPath);
            _logger.LogInformation("Trained and saved initial ML model with simulated data.");
        }

        // ML Prediction (uncomment in EstimateWaitTime when ready)
        private float PredictWithML(OrderData features)
        {
            if (_mlModel == null) return 0f;  // Fallback

            var predictionEngine = _mlContext.Model.CreatePredictionEngine<OrderData, WaitPrediction>(_mlModel);
            var prediction = predictionEngine.Predict(features);
            return Math.Max(1.0f, prediction.WaitMinutes);
        }

        // Helpers
        private List<int> ParseItems(string itemIds) => string.IsNullOrEmpty(itemIds) ? new List<int>() : itemIds.Split(',').Select(int.Parse).ToList();

        public int GetQueuePosition(Order order) => ActiveOrders.IndexOf(order) + 1;

        public OrderData PrepareFeatures(Order order, int queueLength, int itemCount)
        {
            var ordersAhead = ActiveOrders.TakeWhile(o => o != order).ToList();
            var itemsAheadCount = new float[Constants.MaxMenuId];  // Index 0 = ID 1, etc
            int totalItemsAhead = ordersAhead.Sum(o => ParseItems(o.ItemIds).Count);

            // Aggregate counts per item type and store count in array
            foreach (var ahead in ordersAhead)
            {
                var items = ParseItems(ahead.ItemIds);
                foreach (int itemId in items)
                {
                    if (itemId >= 1 && itemId <= Constants.MaxMenuId)
                        itemsAheadCount[itemId - 1]++;  // ID 1 → index 0
                }
            }
            return new OrderData
            {
                ItemCount = itemCount,
                QueueLength = queueLength,
                HourOfDay = order.PlacedAt.Hour,
                DayOfWeek = (int)order.PlacedAt.DayOfWeek,
                TotalItemsAhead = totalItemsAhead,
                ItemsAhead = itemsAheadCount
            };
        }

        private List<OrderData> GenerateSimulatedData(int count)
        {
            var rnd = new Random();
            return Enumerable.Range(1, count).Select(i => new OrderData
            {
                ItemCount = rnd.Next(1, 5),
                QueueLength = rnd.Next(0, 10),
                HourOfDay = rnd.Next(0, 23),
                DayOfWeek = rnd.Next(0, 6),
                TotalItemsAhead = rnd.Next(0, 20),
                ItemsAhead = Enumerable.Range(0, Constants.MaxMenuId).Select(_ => rnd.Next(0, 3)).Select(x => (float)x).ToArray(),
                WaitMinutes = (float)(rnd.Next(5, 30) + rnd.NextDouble())  // Fake labels 5-30 min
            }).ToList();
        }

        //=============================================
        //==== For testing: Expose internal queues ====
        //=============================================

        // Returns currently active orders in queue
        public IReadOnlyList<Order> GetActiveQueue()
        {
            return ActiveOrders.AsReadOnly();
        }

        // Returns orders that are completed but not yet used for ML retraining
        public IReadOnlyList<Order> GetPendingTrainingOrders()
        {
            return CompletedPendingTraining.AsReadOnly();
        }

        // Returns orders that have already been used to retrain the model
        public IReadOnlyList<Order> GetTrainedOrders()
        {
            return TrainedOrders.AsReadOnly();
        }

        // Bonus: Pretty-print all queues to console (great for debugging)
        public void PrintAllQueues()
        {
            _logger.LogInformation("=== CURRENT SYSTEM STATE ===");
            _logger.LogInformation($"ACTIVE QUEUE ({ActiveOrders.Count}):");
            foreach (var o in ActiveOrders)
                _logger.LogInformation($"  [{o.PlaceInQueue}] {o.Uid} | Items: {o.ItemIds} | Est: {o.EstimatedWaitTime ?? 0:F1} min");

            _logger.LogInformation($"PENDING TRAINING ({CompletedPendingTraining.Count}):");
            foreach (var o in CompletedPendingTraining)
            {
                var actual = o.CompletedAt.GetValueOrDefault() - o.PlacedAt;
                _logger.LogInformation($"  {o.Uid} | Actual: {actual.TotalMinutes:F1} min | Est: {o.EstimatedWaitTime ?? 0:F1} min");
            }

            _logger.LogInformation($"TRAINED & ARCHIVED ({TrainedOrders.Count}):");
            foreach (var o in TrainedOrders)
                _logger.LogInformation($"  {o.Uid}");
            _logger.LogInformation("=== END STATE ===");
        }
    }

    // ML Data Classes
    public class OrderData
    {
        public float ItemCount { get; set; } // Item count in the order
        public float QueueLength { get; set; } // Number of orders ahead in the queue
        public float HourOfDay { get; set; }
        public float DayOfWeek { get; set; }
        public float TotalItemsAhead { get; set; } // Total number of items ahead in the queue
        public float WaitMinutes { get; set; }
        // This is an array that holds counts of each item type ahead in the queue at the time the order was placed. This helps the model understand the weight of different items
        [VectorType(Constants.MaxMenuId)]  // ← Tells ML.NET this is a fixed-size vector
        public float[] ItemsAhead { get; set; } = new float[Constants.MaxMenuId];
    }

    public class WaitPrediction
    {
        [ColumnName("Score")]
        public float WaitMinutes { get; set; }
    }
}