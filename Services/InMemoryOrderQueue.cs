using Microsoft.Extensions.Logging;
using WaitTimeTesting.Models;

namespace WaitTimeTesting.Services
{
    public class InMemoryOrderQueue : IOrderQueue
    {
        private readonly List<Order> _orders = new();
        private readonly ILogger<InMemoryOrderQueue> _logger;

        public InMemoryOrderQueue(ILogger<InMemoryOrderQueue> logger)
        {
            _logger = logger;
        }

        public void Add(Order order)
        {
            if (_orders.Any(o => o.Uid == order.Uid))
                throw new InvalidOperationException("Duplicate order UID.");
            _orders.Add(order);
            _logger.LogInformation($"Order {order.Uid} added. Queue: {_orders.Count}");
        }

        public Order Remove(Guid uid)
        {
            var order = _orders.FirstOrDefault(o => o.Uid == uid)
                ?? throw new KeyNotFoundException("Order not found");
            _orders.Remove(order);
            return order;
        }

        public int GetCurrentLength() => _orders.Count;

        public IReadOnlyList<Order> GetActiveOrders() => _orders.AsReadOnly();

        public Order? FindById(Guid uid) => _orders.FirstOrDefault(o => o.Uid == uid);

        public int GetPosition(Order order) => _orders.IndexOf(order) + 1;
    }
}