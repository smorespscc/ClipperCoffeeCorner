using Microsoft.EntityFrameworkCore;
using WaitTimeTesting.Data;
using WaitTimeTesting.Models;

// Isn't currently in use, using InMemoryOrderQueue instead. If/when we have a DB up we can switch to this

namespace WaitTimeTesting.Services
{
    public class DbOrderQueue : IOrderQueue
    {
        private readonly AppDbContext _db;
        private readonly ILogger<DbOrderQueue> _logger;

        public DbOrderQueue(AppDbContext db, ILogger<DbOrderQueue> logger)
        {
            _db = db;
            _logger = logger;
        }

        public void Add(Order order)
        {
            _db.ActiveOrders.Add(order);
            _db.SaveChanges();
            _logger.LogInformation($"Order {order.Uid} saved to DB queue");
        }

        public Order Remove(Guid uid)
        {
            var order = _db.ActiveOrders.FirstOrDefault(o => o.Uid == uid)
                ?? throw new KeyNotFoundException("Order not found");
            _db.ActiveOrders.Remove(order);
            _db.SaveChanges();
            return order;
        }

        public int GetCurrentLength() => _db.ActiveOrders.Count();

        public IReadOnlyList<Order> GetActiveOrders() => _db.ActiveOrders.AsNoTracking().ToList();

        public Order? FindById(Guid uid) => _db.ActiveOrders.FirstOrDefault(o => o.Uid == uid);

        public int GetPosition(Order order)
        {
            // Recalculate based on PlacedAt (most fair)
            return _db.ActiveOrders
                .Where(o => o.PlacedAt <= order.PlacedAt)
                .Count();
        }
    }
}