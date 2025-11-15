using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using WaitTimeTesting.Models;

// SMS notification service using Twilio

namespace WaitTimeTesting.Services
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

        public async Task SendAsync(Order order, NotificationType type)
        {
            if (order.NotificationPref != NotificationPreference.Sms || string.IsNullOrEmpty(order.PhoneNumber))
                return;

            var message = type == NotificationType.Placement
                ? $"Order placed! Est. wait: {(int)(order.EstimatedWaitTime ?? 0)} min"
                : $"Your order is ready!";

            try
            {
                var result = await MessageResource.CreateAsync(
                    body: message,
                    from: new PhoneNumber(_fromNumber),
                    to: new PhoneNumber(order.PhoneNumber)
                );
                _logger.LogInformation($"SMS sent to {order.PhoneNumber}: {result.Sid}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMS failed");
            }
        }
    }
}
