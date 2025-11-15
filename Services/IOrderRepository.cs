using ClipperCoffeeCorner.Models;

namespace ClipperCoffeeCorner.Services
{
    public interface IOrderRepository
    {
        void Add(Order order);
        Order CompleteOrder(Guid uid);
        Order? FindById(Guid uid);

        IReadOnlyList<Order> GetActiveOrders();
        IReadOnlyList<Order> GetCompletedOrders();

        int GetCurrentLength();
        int GetPosition(Order order);
    }
}
