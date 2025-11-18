using ClipperCoffeeCorner.Models;

namespace ClipperCoffeeCorner.Services
{
    public interface INotificationService
    {
        Task SendPlacedAsync(Order order, User user, double estimatedWaitTime);
        Task SendCompletionAsync(Order order, User user);
    }
}
