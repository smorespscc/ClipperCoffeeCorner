using WaitTimeTesting.Models;

namespace WaitTimeTesting.Services
{
    public interface IOrderQueue
    {
        void Add(Order order);
        Order Remove(Guid uid);
        int GetCurrentLength();
        IReadOnlyList<Order> GetActiveOrders();
        Order? FindById(Guid uid);
        int GetPosition(Order order);
    }
}