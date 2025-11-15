using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using WaitTimeTesting.Models;
using WaitTimeTesting.Options;

namespace WaitTimeTesting.Services
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

        public async Task SendAsync(Order order, NotificationType type)
        {
            if (!order.NotificationPref.HasFlag(NotificationPreference.Email) ||
                string.IsNullOrWhiteSpace(order.Email))
                return;

            // construct message. Gotta add some stuff to actually find and display names of items
            var subject = type == NotificationType.Placement ? "Order Placed!" : "Order Ready!";
            var plain = type == NotificationType.Placement
                ? $"Items: {order.ItemIds}\nPosition: {order.PlaceInQueue}\nEst. wait: {(int)(order.EstimatedWaitTime ?? 0)} min"
                : $"Your order {order.Uid} is ready!";

            var html = type == NotificationType.Placement
                ? $"<p>Items: <strong>{order.ItemIds}</strong></p>" +
                  $"<p>Position: <strong>{order.PlaceInQueue}</strong></p>" +
                  $"<p>Est. wait: <strong>{(int)(order.EstimatedWaitTime ?? 0)} min</strong></p>"
                : $"<p>Your order <strong>{order.Uid}</strong> is ready!</p>";

            var msg = new SendGridMessage
            {
                From = new EmailAddress(_opts.FromEmail, _opts.FromName),
                Subject = subject,
                PlainTextContent = plain,
                HtmlContent = html
            };
            msg.AddTo(order.Email);

            try
            {
                var resp = await _client.SendEmailAsync(msg);
                if (resp.IsSuccessStatusCode)
                    _logger.LogInformation($"[EMAIL] Sent to {order.Email}");
                else
                    _logger.LogError($"[EMAIL] Failed: {await resp.Body.ReadAsStringAsync()}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[EMAIL] Exception to {order.Email}");
            }
        }
    }
}