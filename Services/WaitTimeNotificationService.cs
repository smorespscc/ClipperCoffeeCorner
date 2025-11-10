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
        private readonly IOrderQueue _queue;
        private readonly IWaitTimeEstimator _estimator;
        private readonly INotificationService _notifier;
        private readonly IOrderStorage _storage;
        private readonly ILogger<WaitTimeNotificationService> _logger;

        public WaitTimeNotificationService(
            IOrderQueue queue,
            IWaitTimeEstimator estimator,
            INotificationService notifier,
            IOrderStorage storage,
            ILogger<WaitTimeNotificationService> logger)
        {
            _queue = queue;
            _estimator = estimator;
            _notifier = notifier;
            _storage = storage;
            _logger = logger;
        }

        public async Task AddOrderAsync(Order order)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));

            order.PlacedAt = DateTimeOffset.Now;
            order.Status = OrderStatus.Pending;
            order.PlaceInQueue = _queue.GetCurrentLength() + 1;

            // Estimate wait time + capture ML features
            var (waitTime, features) = _estimator.Estimate(order, _queue);

            order.EstimatedWaitTime = waitTime;
            order.ItemsAheadAtPlacement = features.ItemsAhead;
            order.TotalItemsAheadAtPlacement = features.TotalItemsAhead;

            // Persist to queue (DB or memory)
            _queue.Add(order);

            _logger.LogInformation(
                "Order {OrderId} placed | Items: {Items} | Position: {Position} | Est. wait: {Wait:F1} min",
                order.Uid, order.ItemIds, order.PlaceInQueue, waitTime);

            // Send confirmation SMS
            if (order.NotificationPref == NotificationPreference.Sms && !string.IsNullOrEmpty(order.PhoneNumber))
            {
                await _notifier.SendAsync(order, NotificationType.Placement);
            }
        }

        public async Task<Order> CompleteOrderAsync(Guid orderId, DateTimeOffset? completedAt = null)
        {
            var order = _queue.FindById(orderId)
                ?? throw new KeyNotFoundException($"Order {orderId} not found in active queue.");

            // Remove from active queue
            _queue.Remove(orderId);

            // Finalize completion
            order.CompletedAt = completedAt ?? DateTimeOffset.Now;
            order.Status = OrderStatus.Complete;

            double actualWait = (order.CompletedAt.Value - order.PlacedAt).TotalMinutes;
            double error = Math.Abs(actualWait - (order.EstimatedWaitTime ?? 0));

            _logger.LogInformation(
                "Order {OrderId} completed | Actual wait: {Actual:F1} min | Est: {Est:F1} min | Error: {Error:F1} min",
                order.Uid, actualWait, order.EstimatedWaitTime, error);

            // Send ready SMS
            if (order.NotificationPref == NotificationPreference.Sms && !string.IsNullOrEmpty(order.PhoneNumber))
            {
                await _notifier.SendAsync(order, NotificationType.Completion);
            }

            // Persist completed order for analytics
            _storage.StoreCompleted(order);

            // Feed into ML retraining system
            _estimator.AddCompletedForTraining(order);

            return order;
        }

        public void ForceRetrain() => _estimator.RetrainIfNeeded();
        public IReadOnlyList<Order> GetActiveQueue() => _queue.GetActiveOrders();
        public int GetCurrentQueueLength() => _queue.GetCurrentLength();
        public int GetQueuePosition(Order order) => _queue.GetPosition(order);

        //=============================================
        //==== For testing: Expose internal queues ====
        //=============================================

        public void PrintSystemState()
        {
            _logger.LogInformation("=== CAFE WAIT SYSTEM STATE ===");
            _logger.LogInformation($"Active Orders: {_queue.GetCurrentLength()}");

            foreach (var o in _queue.GetActiveOrders().OrderBy(o => o.PlacedAt))
            {
                var pos = _queue.GetPosition(o);
                _logger.LogInformation(
                    " [{Pos}] {Uid} | {Items} | Est: {Est:F1} min | Placed: {Ago} ago",
                    pos, o.Uid, o.ItemIds, o.EstimatedWaitTime,
                    FormatTimeAgo(o.PlacedAt));
            }
            _logger.LogInformation("=== END STATE ===");
        }

        private string FormatTimeAgo(DateTimeOffset dt)
        {
            var span = DateTimeOffset.Now - dt;
            return span.TotalMinutes < 60
                ? $"{span.TotalMinutes:F0}m ago"
                : $"{span.TotalHours:F1}h ago";
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
        [VectorType(Constants.MaxMenuId)]
        public float[] ItemsAhead { get; set; } = new float[Constants.MaxMenuId];
    }

    public class WaitPrediction
    {
        [ColumnName("Score")]
        public float WaitMinutes { get; set; }
    }
}