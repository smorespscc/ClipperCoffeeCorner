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
        private readonly IWaitTimeEstimator _estimator;
        private readonly IEnumerable<INotificationService> _notifier;
        private readonly ILogger<WaitTimeNotificationService> _logger;
        private readonly IOrderRepository _orders;

        public WaitTimeNotificationService(
            IOrderRepository orders,
            IWaitTimeEstimator estimator,
            IEnumerable<INotificationService> notifier,
            ILogger<WaitTimeNotificationService> logger)
        {
            _orders = orders;
            _estimator = estimator;
            _notifier = notifier;
            _logger = logger;
        }

        public async Task AddOrderAsync(Order order)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));

            // set order fields
            order.PlacedAt = DateTimeOffset.Now;
            order.Status = OrderStatus.Pending;
            order.PlaceInQueue = _orders.GetCurrentLength() + 1;

            // ask ML estimation service for wait time (can change this depending on what ML service actually needs)
            // the "features" stuff might be unnecessary and can be removed so it just returns an estimated wait time
            var (waitTime, features) = _estimator.Estimate(order, _orders);

            // fill in fields from ML estimation stuff
            order.EstimatedWaitTime = waitTime;
            order.ItemsAheadAtPlacement = features.ItemsAhead;            // could be removed
            order.TotalItemsAheadAtPlacement = features.TotalItemsAhead;  // could be removed

            // add to orders table
            _orders.Add(order);

            _logger.LogInformation(
                "Order {OrderId} placed | Items: {Items} | Position: {Position} | Est. wait: {Wait:F1} min",
                order.Uid, order.ItemIds, order.PlaceInQueue, waitTime);

            // send confirmation SMS or Email
            foreach (var notifier in _notifier)
            {
                await notifier.SendAsync(order, NotificationType.Placement);
            }
        }



        public async Task<Order> CompleteOrderAsync(Guid orderId)
        {
            // retrieve order from sql table
            var order = _orders.FindById(orderId)
                ?? throw new KeyNotFoundException($"Order {orderId} not found in active queue.");

            // set completion fields
            order.CompletedAt = DateTimeOffset.Now;
            order.Status = OrderStatus.Complete;
            double actualWait = (order.CompletedAt.Value - order.PlacedAt).TotalMinutes;  // probably won't use
            double error = Math.Abs(actualWait - (order.EstimatedWaitTime ?? 0));         // probably won't use

            // complete order by updating table
            _orders.CompleteOrder(orderId);

            _logger.LogInformation(
                "Order {OrderId} completed | Actual wait: {Actual:F1} min | Est: {Est:F1} min | Error: {Error:F1} min",
                order.Uid, actualWait, order.EstimatedWaitTime, error);

            // Send ready SMS or Email
            foreach (var notifier in _notifier)
            {
                await notifier.SendAsync(order, NotificationType.Completion);
            }

            // give to ML training service. Might not need this.
            _estimator.AddCompletedForTraining(order);

            return order;
        }



        //=============================================
        //==== For testing stuff. Can delete later ====
        //=============================================

        public void ForceRetrain() => _estimator.RetrainIfNeeded();
        public IReadOnlyList<Order> GetActiveQueue() => _orders.GetActiveOrders();
        public int GetCurrentQueueLength() => _orders.GetCurrentLength();
        public int GetQueuePosition(Order order) => _orders.GetPosition(order);

        public void PrintSystemState()
        {
            _logger.LogInformation("=== CAFE WAIT SYSTEM STATE ===");
            _logger.LogInformation($"Active Orders: {_orders.GetCurrentLength()}");

            foreach (var o in _orders.GetActiveOrders().OrderBy(o => o.PlacedAt))
            {
                var pos = _orders.GetPosition(o);
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
    // This will be replaced by whatever the actual ML service needs
    public class OrderData
    {
        public float ItemCount { get; set; } // Item count in the order
        public float QueueLength { get; set; } // Number of orders ahead in the queue
        public float HourOfDay { get; set; }
        public float DayOfWeek { get; set; }
        public float TotalItemsAhead { get; set; } // Total number of items ahead in the queue
        public float WaitMinutes { get; set; }
        [VectorType(Constants.MaxMenuId)]
        public float[] ItemsAhead { get; set; } = new float[Constants.MaxMenuId];
    }

    public class WaitPrediction
    {
        [ColumnName("Score")]
        public float WaitMinutes { get; set; }
    }
}