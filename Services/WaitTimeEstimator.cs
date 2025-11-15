using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers.FastTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WaitTimeTesting.Models;
using WaitTimeTesting.Options;

namespace WaitTimeTesting.Services
{
    public interface IWaitTimeEstimator
    {
        (double WaitTime, OrderData Features) Estimate(Order order, IOrderRepository queue);
        void AddCompletedForTraining(Order order);
        void RetrainIfNeeded();
    }

    public class WaitTimeEstimator : IWaitTimeEstimator
    {
        private readonly MLContext _mlContext = new MLContext(seed: 0);
        private ITransformer? _model;
        private readonly string _modelPath = "model.zip";
        private readonly ILogger<WaitTimeEstimator> _logger;

        // In-memory buffer for completed orders pending retraining
        private readonly List<Order> _completedPendingTraining = new();
        private const int RetrainThreshold = 100; // Adjust in production

        public WaitTimeEstimator(ILogger<WaitTimeEstimator> logger)
        {
            _logger = logger;
            LoadOrTrainInitialModel();
        }

        public (double WaitTime, OrderData Features) Estimate(Order order, IOrderRepository queue)
        {
            var items = ParseItems(order.ItemIds);
            int itemCount = items.Count;
            int queueLength = queue.GetCurrentLength(); // Includes current order

            var features = PrepareFeatures(order, queueLength, itemCount, queue);
            double estimatedMinutes = PredictWithML(features);

            _logger.LogInformation($"Estimated wait for order {order.Uid}: {estimatedMinutes:F1} minutes");
            return (estimatedMinutes, features);
        }

        public void AddCompletedForTraining(Order order)
        {
            if (!order.CompletedAt.HasValue || !order.EstimatedWaitTime.HasValue)
            {
                _logger.LogWarning($"Order {order.Uid} missing completion data. Skipping training.");
                return;
            }

            _completedPendingTraining.Add(order);

            double actual = (order.CompletedAt.Value - order.PlacedAt).TotalMinutes;
            double error = Math.Abs(actual - order.EstimatedWaitTime.Value);
            _logger.LogInformation($"Training data added: Order {order.Uid} | Actual: {actual:F1}m | Est: {order.EstimatedWaitTime:F1}m | Error: {error:F1}m");

            if (_completedPendingTraining.Count >= RetrainThreshold)
            {
                RetrainIfNeeded();
            }
        }

        public void RetrainIfNeeded()
        {
            if (_completedPendingTraining.Count == 0)
                return;

            _logger.LogInformation($"Retraining model with {_completedPendingTraining.Count} new completed orders...");

            var trainingData = _mlContext.Data.LoadFromEnumerable(
                _completedPendingTraining.Select(MapToOrderData)
            );

            var pipeline = _mlContext.Transforms.CopyColumns("Label", nameof(OrderData.WaitMinutes))
                .Append(_mlContext.Transforms.Concatenate("Features",
                    nameof(OrderData.ItemCount),
                    nameof(OrderData.QueueLength),
                    nameof(OrderData.HourOfDay),
                    nameof(OrderData.DayOfWeek),
                    nameof(OrderData.TotalItemsAhead),
                    nameof(OrderData.ItemsAhead)))
                .Append(_mlContext.Regression.Trainers.FastTree(
                    labelColumnName: "Label",
                    featureColumnName: "Features"));

            _model = pipeline.Fit(trainingData);

            // Evaluate
            var predictions = _model.Transform(trainingData);
            var metrics = _mlContext.Regression.Evaluate(predictions, "Label");
            _logger.LogInformation($"Model retrained! R²: {metrics.RSquared:F4} | RMSE: {metrics.RootMeanSquaredError:F2}");

            // Save
            _mlContext.Model.Save(_model, trainingData.Schema, _modelPath);
            _logger.LogInformation("New model saved to model.zip");

            // Clear buffer
            _completedPendingTraining.Clear();
        }

        private float PredictWithML(OrderData features)
        {
            if (_model == null)
            {
                _logger.LogWarning("ML model not loaded. Using fallback.");
                return 10f; // fallback
            }

            var engine = _mlContext.Model.CreatePredictionEngine<OrderData, WaitPrediction>(_model);
            var prediction = engine.Predict(features);
            return Math.Max(1.0f, prediction.WaitMinutes);
        }

        private void LoadOrTrainInitialModel()
        {
            if (File.Exists(_modelPath))
            {
                try
                {
                    _model = _mlContext.Model.Load(_modelPath, out var schema);
                    _logger.LogInformation("ML model loaded from model.zip");
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load model. Training new one.");
                }
            }

            _logger.LogInformation("No model found. Training initial model with simulated data...");
            var simulated = GenerateSimulatedData(100);
            var dataView = _mlContext.Data.LoadFromEnumerable(simulated);

            var pipeline = _mlContext.Transforms.CopyColumns("Label", nameof(OrderData.WaitMinutes))
                .Append(_mlContext.Transforms.Concatenate("Features",
                    nameof(OrderData.ItemCount),
                    nameof(OrderData.QueueLength),
                    nameof(OrderData.HourOfDay),
                    nameof(OrderData.DayOfWeek),
                    nameof(OrderData.TotalItemsAhead),
                    nameof(OrderData.ItemsAhead)))
                .Append(_mlContext.Regression.Trainers.FastTree());

            _model = pipeline.Fit(dataView);
            _mlContext.Model.Save(_model, dataView.Schema, _modelPath);
            _logger.LogInformation("Initial ML model trained and saved.");
        }

        private OrderData PrepareFeatures(Order order, int queueLength, int itemCount, IOrderRepository queue)
        {
            var ordersAhead = queue.GetActiveOrders()
                .Where(o => o.PlacedAt < order.PlacedAt || (o.PlacedAt == order.PlacedAt && o.Uid != order.Uid))
                .ToList();

            var itemsAheadCount = new float[Constants.MaxMenuId];
            int totalItemsAhead = 0;

            foreach (var ahead in ordersAhead)
            {
                var items = ParseItems(ahead.ItemIds);
                totalItemsAhead += items.Count;
                foreach (int itemId in items)
                {
                    if (itemId >= 1 && itemId <= Constants.MaxMenuId)
                        itemsAheadCount[itemId - 1]++;
                }
            }

            return new OrderData
            {
                ItemCount = itemCount,
                QueueLength = queueLength,
                HourOfDay = order.PlacedAt.Hour,
                DayOfWeek = (int)order.PlacedAt.DayOfWeek,
                TotalItemsAhead = totalItemsAhead,
                ItemsAhead = itemsAheadCount,
                WaitMinutes = 0 // not used in prediction
            };
        }

        private OrderData MapToOrderData(Order order)
        {
            var items = ParseItems(order.ItemIds);
            return new OrderData
            {
                ItemCount = items.Count,
                QueueLength = order.PlaceInQueue,
                HourOfDay = order.PlacedAt.Hour,
                DayOfWeek = (int)order.PlacedAt.DayOfWeek,
                ItemsAhead = order.ItemsAheadAtPlacement,
                TotalItemsAhead = order.TotalItemsAheadAtPlacement,
                WaitMinutes = (float)(order.CompletedAt!.Value - order.PlacedAt).TotalMinutes
            };
        }

        private List<int> ParseItems(string itemIds) =>
            string.IsNullOrWhiteSpace(itemIds)
                ? new List<int>()
                : itemIds.Split(',').Select(int.Parse).ToList();

        private List<OrderData> GenerateSimulatedData(int count)
        {
            var rnd = new Random(42);
            return Enumerable.Range(0, count).Select(_ => new OrderData
            {
                ItemCount = rnd.Next(1, 6),
                QueueLength = rnd.Next(0, 15),
                HourOfDay = rnd.Next(6, 22),
                DayOfWeek = rnd.Next(0, 7),
                TotalItemsAhead = rnd.Next(0, 30),
                ItemsAhead = Enumerable.Range(0, Constants.MaxMenuId)
                    .Select(_ => (float)rnd.Next(0, 5))
                    .ToArray(),
                WaitMinutes = (float)(rnd.Next(5, 35) + rnd.NextDouble() * 5)
            }).ToList();
        }
    }
}