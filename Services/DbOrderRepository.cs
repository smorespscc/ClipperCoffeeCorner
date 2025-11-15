using Microsoft.EntityFrameworkCore;
using WaitTimeTesting.Data;
using WaitTimeTesting.Models;

// Isn't currently in use, using InMemoryOrderQueue instead. If/when we have a DB up we can switch to this. Or scrap it and use whatever DB team makes idk.

namespace WaitTimeTesting.Services
{
    public class DbOrderRepository : IOrderRepository
    {
        private readonly AppDbContext _db;
        private readonly ILogger<DbOrderRepository> _logger;

        public DbOrderRepository(AppDbContext db, ILogger<DbOrderRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public void Add(Order order)
        {
            _db.Orders.Add(order);
            _db.SaveChanges();
        }

        public Order CompleteOrder(Guid uid)
        {
            var order = _db.Orders.FirstOrDefault(o => o.Uid == uid)
                ?? throw new KeyNotFoundException("Order not found");

            if (order.Status == OrderStatus.Complete)
                return order;

            order.CompletedAt = DateTimeOffset.Now;
            order.Status = OrderStatus.Complete;
            order.ActualWaitMinutes = (float)(order.CompletedAt.Value - order.PlacedAt).TotalMinutes;
            order.PredictionError = Math.Abs(order.ActualWaitMinutes.Value - (float)(order.EstimatedWaitTime ?? 0));

            _db.SaveChanges();
            return order;
        }

        public Order? FindById(Guid uid) =>
            _db.Orders.FirstOrDefault(o => o.Uid == uid);

        public IReadOnlyList<Order> GetActiveOrders() =>
            _db.Orders
              .AsNoTracking()
              .Where(o => o.Status == OrderStatus.Pending)
              .OrderBy(o => o.PlacedAt)
              .ToList();

        public IReadOnlyList<Order> GetCompletedOrders() =>
            _db.Orders
              .AsNoTracking()
              .Where(o => o.Status == OrderStatus.Complete)
              .OrderBy(o => o.CompletedAt)
              .ToList();

        public int GetCurrentLength() =>
            _db.Orders.Count(o => o.Status == OrderStatus.Pending);

        public int GetPosition(Order order) =>
            _db.Orders.Count(o => o.Status == OrderStatus.Pending && o.PlacedAt <= order.PlacedAt);
    }

    public class InMemoryOrderRepository : IOrderRepository
    {
        private readonly List<Order> _orders = new();
        private readonly ILogger<InMemoryOrderRepository> _logger;

        public InMemoryOrderRepository(ILogger<InMemoryOrderRepository> logger)
        {
            _logger = logger;
        }

        public void Add(Order order)
        {
            _orders.Add(order);
        }

        public Order CompleteOrder(Guid uid)
        {
            var o = _orders.FirstOrDefault(x => x.Uid == uid)
                ?? throw new KeyNotFoundException("Order not found");

            o.CompletedAt = DateTimeOffset.Now;
            o.Status = OrderStatus.Complete;
            return o;
        }

        public Order? FindById(Guid uid) =>
            _orders.FirstOrDefault(o => o.Uid == uid);

        public IReadOnlyList<Order> GetActiveOrders() =>
            _orders.Where(o => o.Status == OrderStatus.Pending).ToList();

        public IReadOnlyList<Order> GetCompletedOrders() =>
            _orders.Where(o => o.Status == OrderStatus.Complete).ToList();

        public int GetCurrentLength() =>
            _orders.Count(o => o.Status == OrderStatus.Pending);

        public int GetPosition(Order order) =>
            _orders.Count(o =>
                o.Status == OrderStatus.Pending &&
                o.PlacedAt <= order.PlacedAt
            );
    }
}