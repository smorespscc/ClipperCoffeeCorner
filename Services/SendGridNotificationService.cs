using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using ClipperCoffeeCorner.Models;
using ClipperCoffeeCorner.Options;

// Email notification service using SendGrid

namespace ClipperCoffeeCorner.Services
{
    public class SendGridNotificationService : INotificationService
    {
        private readonly SendGridClient _client;
        private readonly SendGridOptions _opts;
        private readonly ILogger<SendGridNotificationService> _logger;

        public SendGridNotificationService(
            IOptions<SendGridOptions> options,
            ILogger<SendGridNotificationService> logger)
        {
            _opts = options.Value;
            _client = new SendGridClient(_opts.ApiKey);
            _logger = logger;
        }

        // Send order placement notification
        public async Task SendPlacedAsync(Order order, User user, double estimatedWaitTime)
        {
            if (!user.NotificationPref.HasFlag(NotificationPreference.Email) ||
                string.IsNullOrWhiteSpace(user.NotificationEmail))
                return;

            var subject = "Order Placed!";
            var plain = $"Items: {order.LineItems}\nPosition in line: idk dawg maybe we can add this\nEst. wait: {estimatedWaitTime} min";
            var html = $"<p>Items: <strong>{order.LineItems}</strong></p>" +
                       $"<p>Position: <strong>idk dawg maybe we can add this</strong></p>" +
                       $"<p>Est. wait: <strong>{estimatedWaitTime} min</strong></p>";

            var msg = new SendGridMessage
            {
                From = new EmailAddress(_opts.FromEmail, _opts.FromName),
                Subject = subject,
                PlainTextContent = plain,
                HtmlContent = html
            };
            msg.AddTo(user.NotificationEmail);

            try
            {
                var resp = await _client.SendEmailAsync(msg);
                if (resp.IsSuccessStatusCode)
                    _logger.LogInformation($"[EMAIL] Sent to {user.NotificationEmail}");
                else
                    _logger.LogError($"[EMAIL] Failed: {await resp.Body.ReadAsStringAsync()}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[EMAIL] Exception to {user.NotificationEmail}");
            }
        }

        // Send order completion notification
        public async Task SendCompletionAsync(Order order, User user)
        {
            if (!user.NotificationPref.HasFlag(NotificationPreference.Email) ||
                string.IsNullOrWhiteSpace(user.NotificationEmail))
                return;

            var subject = "Order Ready!";
            var plain = $"Your order {order.OrderId} is ready!";
            var html = $"<p>Your order <strong>{order.OrderId}</strong> is ready!</p>";

            var msg = new SendGridMessage
            {
                From = new EmailAddress(_opts.FromEmail, _opts.FromName),
                Subject = subject,
                PlainTextContent = plain,
                HtmlContent = html
            };
            msg.AddTo(user.NotificationEmail);

            try
            {
                var resp = await _client.SendEmailAsync(msg);
                if (resp.IsSuccessStatusCode)
                    _logger.LogInformation($"[EMAIL] Sent to {user.NotificationEmail}");
                else
                    _logger.LogError($"[EMAIL] Failed: {await resp.Body.ReadAsStringAsync()}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[EMAIL] Exception to {user.NotificationEmail}");
            }
        }
    }
}