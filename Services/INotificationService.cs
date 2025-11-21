using ClipperCoffeeCorner.Models;
using static ClipperCoffeeCorner.Services.WaitTimeNotificationService;

namespace ClipperCoffeeCorner.Services
{
    public interface INotificationService
    {
        Task SendPlacedAsync(Order order, UserResponse user, double estimatedWaitTime);
        Task SendCompletionAsync(OrderDetailsDto order, UserResponse user);
    }
}
