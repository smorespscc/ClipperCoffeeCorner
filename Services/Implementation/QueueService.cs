using ClipperCoffeeCorner.Models.Domain;
using ClipperCoffeeCorner.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClipperCoffeeCorner.Services.Implementation
{
    /// <summary>
    /// Implementation of queue management business logic.
    /// Handles queue display, position tracking, and wait time estimation.
    /// </summary>
    public class QueueService : IQueueService
    {
        private readonly IMemoryCache _cache;
        private const string QueueCacheKey = "current_queue";

        public QueueService(IMemoryCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        // ==================== QUEUE MANAGEMENT ====================

        public Task<List<QueueEntry>> GetCurrentQueueAsync()
        {
            if (_cache.TryGetValue(QueueCacheKey, out List<QueueEntry>? queue))
            {
                return Task.FromResult(queue ?? new List<QueueEntry>());
            }

            // Return empty queue if none exists
            return Task.FromResult(new List<QueueEntry>());
        }

        public async Task<int?> GetCustomerQueuePositionAsync(int orderNumber)
        {
            var queue = await GetCurrentQueueAsync();
            var entry = queue.FirstOrDefault(e => e.OrderNumber == orderNumber);

            return entry?.QueuePosition;
        }

        public async Task<int> EstimateWaitTimeAsync(int orderNumber)
        {
            var queue = await GetCurrentQueueAsync();
            var entry = queue.FirstOrDefault(e => e.OrderNumber == orderNumber);

            if (entry == null) return 0;

            // Simple estimation: 2 minutes per order ahead in queue
            var ordersAhead = queue.Count(e => e.QueuePosition < entry.QueuePosition && e.Status != QueueStatus.Ready);
            return ordersAhead * 2;
        }

        public async Task<bool> AddToQueueAsync(int orderNumber, string itemDescription)
        {
            var queue = await GetCurrentQueueAsync();

            // Check if already in queue
            if (queue.Any(e => e.OrderNumber == orderNumber))
                return false;

            var newEntry = new QueueEntry
            {
                Id = queue.Count + 1,
                OrderNumber = orderNumber,
                ItemDescription = itemDescription,
                Status = QueueStatus.Queued,
                QueuePosition = queue.Count + 1,
                EstimatedWaitTime = queue.Count * 2,
                AddedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            queue.Add(newEntry);
            _cache.Set(QueueCacheKey, queue, TimeSpan.FromHours(4));

            return true;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderNumber, QueueStatus status)
        {
            var queue = await GetCurrentQueueAsync();
            var entry = queue.FirstOrDefault(e => e.OrderNumber == orderNumber);

            if (entry == null) return false;

            entry.Status = status;
            entry.UpdatedAt = DateTime.UtcNow;

            _cache.Set(QueueCacheKey, queue, TimeSpan.FromHours(4));

            return true;
        }

        public async Task<bool> RemoveFromQueueAsync(int orderNumber)
        {
            var queue = await GetCurrentQueueAsync();
            var entry = queue.FirstOrDefault(e => e.OrderNumber == orderNumber);

            if (entry == null) return false;

            queue.Remove(entry);

            // Recalculate positions
            for (int i = 0; i < queue.Count; i++)
            {
                queue[i].QueuePosition = i + 1;
            }

            _cache.Set(QueueCacheKey, queue, TimeSpan.FromHours(4));

            return true;
        }

        // ==================== QUEUE DISPLAY ====================

        public Task<List<QueueEntry>> GeneratePlaceholderQueueAsync(int baseOrderNumber = 200)
        {
            var queue = new List<QueueEntry>
            {
                new QueueEntry
                {
                    Id = 1,
                    OrderNumber = baseOrderNumber - 3,
                    ItemDescription = "Cappuccino",
                    Status = QueueStatus.Preparing,
                    QueuePosition = 1,
                    EstimatedWaitTime = 2
                },
                new QueueEntry
                {
                    Id = 2,
                    OrderNumber = baseOrderNumber - 2,
                    ItemDescription = "Latte",
                    Status = QueueStatus.Ready,
                    QueuePosition = 2,
                    EstimatedWaitTime = 0
                },
                new QueueEntry
                {
                    Id = 3,
                    OrderNumber = baseOrderNumber - 1,
                    ItemDescription = "Matcha Tea",
                    Status = QueueStatus.Queued,
                    QueuePosition = 3,
                    EstimatedWaitTime = 4
                },
                new QueueEntry
                {
                    Id = 4,
                    OrderNumber = baseOrderNumber,
                    ItemDescription = "Americano",
                    Status = QueueStatus.Preparing,
                    QueuePosition = 4,
                    EstimatedWaitTime = 6
                },
                new QueueEntry
                {
                    Id = 5,
                    OrderNumber = baseOrderNumber + 1,
                    ItemDescription = "Espresso",
                    Status = QueueStatus.Queued,
                    QueuePosition = 5,
                    EstimatedWaitTime = 8
                },
                new QueueEntry
                {
                    Id = 6,
                    OrderNumber = baseOrderNumber + 2,
                    ItemDescription = "Mocha",
                    Status = QueueStatus.Preparing,
                    QueuePosition = 6,
                    EstimatedWaitTime = 10
                },
                new QueueEntry
                {
                    Id = 7,
                    OrderNumber = baseOrderNumber + 3,
                    ItemDescription = "Flat White",
                    Status = QueueStatus.Queued,
                    QueuePosition = 7,
                    EstimatedWaitTime = 12
                }
            };

            return Task.FromResult(queue);
        }

        public async Task<bool> IsOrderReadyAsync(int orderNumber)
        {
            var queue = await GetCurrentQueueAsync();
            var entry = queue.FirstOrDefault(e => e.OrderNumber == orderNumber);

            return entry?.Status == QueueStatus.Ready;
        }
    }
}
