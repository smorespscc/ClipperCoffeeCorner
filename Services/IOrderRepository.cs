using ClipperCoffeeCorner.Models;

namespace ClipperCoffeeCorner.Services
{
    public interface IOrderRepository
    {
        void Add(Order order);
        void CompleteOrder(Guid uid);
    }
}
