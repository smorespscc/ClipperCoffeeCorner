using ClipperCoffeeCorner.Models;
using static ClipperCoffeeCorner.Services.WaitTimeNotificationService;

namespace ClipperCoffeeCorner.Services
{
    public interface INotificationService
    {
        Task SendPlacedAsync(OrderDetailsDto order, UserResponse user, double estimatedWaitTime, List<OrderItemDetailsDto> itemDetails);
        Task SendCompletionAsync(OrderDetailsDto order, UserResponse user, List<OrderItemDetailsDto> itemDetails);
    }
}
