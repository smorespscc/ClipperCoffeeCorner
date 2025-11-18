using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using ClipperCoffeeCorner.Models;

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
        public async Task SendPlacedAsync(Order order, User user, double estimatedWaitTime)
        {
            if (user.NotificationPref != NotificationPreference.Sms ||
                string.IsNullOrWhiteSpace(user.PhoneNumber))
                return;

            var message = $"Order placed! Est. wait: {estimatedWaitTime} min";

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
        public async Task SendCompletionAsync(Order order, User user)
        {
            if (user.NotificationPref != NotificationPreference.Sms ||
                string.IsNullOrWhiteSpace(user.PhoneNumber))
                return;

            var message = $"Your order is ready!";

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
