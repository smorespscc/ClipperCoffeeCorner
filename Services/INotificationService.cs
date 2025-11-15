using ClipperCoffeeCorner.Models;

namespace ClipperCoffeeCorner.Services
{
    public interface INotificationService
    {
        Task SendAsync(Order order, NotificationType type);
    }
}
