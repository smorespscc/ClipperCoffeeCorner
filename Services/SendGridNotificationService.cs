using ClipperCoffeeCorner.Models;
using ClipperCoffeeCorner.Options;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using static ClipperCoffeeCorner.Services.WaitTimeNotificationService;

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
        public async Task SendPlacedAsync(Order order, UserResponse user, double estimatedWaitTime)
        {
            if (user.NotificationPref != "Email" ||
                string.IsNullOrWhiteSpace(user.Email))
                return;

            var itemsList = string.Join(", ", order.OrderItems.Select(oi => $"{oi.OrderItemId}"));

            var subject = "Order Placed!";
            var plain = $"Items: {itemsList}\nPosition in line: idk dawg maybe we can add this\nEst. wait: {estimatedWaitTime} min";
            var html = $"<p>Items: <strong>{itemsList}</strong></p>" +
                       $"<p>Position: <strong>idk dawg maybe we can add this</strong></p>" +
                       $"<p>Est. wait: <strong>{estimatedWaitTime} min</strong></p>";

            var msg = new SendGridMessage
            {
                From = new EmailAddress(_opts.FromEmail, _opts.FromName),
                Subject = subject,
                PlainTextContent = plain,
                HtmlContent = html
            };
            msg.AddTo(user.Email);

            try
            {
                var resp = await _client.SendEmailAsync(msg);
                if (resp.IsSuccessStatusCode)
                    _logger.LogInformation($"[EMAIL] Sent to {user.Email}");
                else
                    _logger.LogError($"[EMAIL] Failed: {await resp.Body.ReadAsStringAsync()}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[EMAIL] Exception to {user.Email}");
            }
        }

        // Send order completion notification
        public async Task SendCompletionAsync(OrderDetailsDto order, UserResponse user)
        {
            if (user.NotificationPref != "Email" ||
                string.IsNullOrWhiteSpace(user.Email))
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
            msg.AddTo(user.Email);

            try
            {
                var resp = await _client.SendEmailAsync(msg);
                if (resp.IsSuccessStatusCode)
                    _logger.LogInformation($"[EMAIL] Sent to {user.Email}");
                else
                    _logger.LogError($"[EMAIL] Failed: {await resp.Body.ReadAsStringAsync()}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[EMAIL] Exception to {user.Email}");
            }
        }
    }
}