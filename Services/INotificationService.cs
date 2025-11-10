using WaitTimeTesting.Models;

namespace WaitTimeTesting.Services
{
    public interface INotificationService
    {
        Task SendAsync(Order order, NotificationType type);
    }
}
