using ClipperCoffeeCorner.Models;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using static ClipperCoffeeCorner.Services.WaitTimeNotificationService;

// SMS notification service using Twilio

namespace ClipperCoffeeCorner.Services
{
    public class TwilioNotificationService : INotificationService
    {
        private readonly string _fromNumber;
        private readonly ILogger<TwilioNotificationService> _logger;

        public TwilioNotificationService(IConfiguration config, ILogger<TwilioNotificationService> logger)
        {
            _fromNumber = config["Twilio:FromPhoneNumber"]!;
            _logger = logger;
        }

        // send order placed SMS notification
        public async Task SendPlacedAsync(Order order, UserResponse user, double estimatedWaitTime, List<OrderItemDetailsDto> itemDetails)
        {
            if (user.NotificationPref != "Sms" ||
                string.IsNullOrWhiteSpace(user.PhoneNumber))
                return;

            var itemsList = string.Join(", ",
                itemDetails.Select(i => $"{i.MenuItemName} x{i.Quantity}"));

            var message =
                $"Order placed!\n" +
                $"Items: {itemsList}\n" +
                $"Est. wait: {estimatedWaitTime} min";

            try
            {
                var result = await MessageResource.CreateAsync(
                    body: message,
                    from: new PhoneNumber(_fromNumber),
                    to: new PhoneNumber(user.PhoneNumber)
                );
                _logger.LogInformation($"SMS sent to {user.PhoneNumber}: {result.Sid}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMS failed");
            }
        }

        // send order completed SMS notification
        public async Task SendCompletionAsync(OrderDetailsDto order, UserResponse user, List<OrderItemDetailsDto> itemDetails)
        {
            if (user.NotificationPref != "Sms" ||
                string.IsNullOrWhiteSpace(user.PhoneNumber))
                return;

            var itemsList = string.Join(", ",
                itemDetails.Select(i => $"{i.MenuItemName} x{i.Quantity}"));

            var message =
                $"Your order #{order.OrderId} is ready!\n" +
                $"Items: {itemsList}";

            try
            {
                var result = await MessageResource.CreateAsync(
                    body: message,
                    from: new PhoneNumber(_fromNumber),
                    to: new PhoneNumber(user.PhoneNumber)
                );
                _logger.LogInformation($"SMS sent to {user.PhoneNumber}: {result.Sid}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMS failed");
            }
        }
    }
}
