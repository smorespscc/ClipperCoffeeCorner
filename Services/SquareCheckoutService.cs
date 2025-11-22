using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ClipperCoffeeCorner.Models;

namespace Services
{
    public class SquareCheckoutService : ISquareCheckoutService
    {
        private readonly HttpClient _httpClient;
        private readonly string _accessToken;
        private readonly string _locationId;
        private readonly string _apiVersion;

        public SquareCheckoutService(IHttpClientFactory httpFactory, IConfiguration config)
        {
            _httpClient = httpFactory.CreateClient("Square");
            _accessToken = config["Square:AccessToken"] ?? throw new ArgumentNullException("Square:AccessToken");
            _locationId = config["Square:LocationId"] ?? throw new ArgumentNullException("Square:LocationId");
            _apiVersion = config["Square:ApiVersion"] ?? "2023-08-16";
        }

        public async Task<string> CreatePaymentLinkAsync(Order order, string? redirectUrl = null)
        {
            var idempotencyKey = order.IdempotencyKey;
            var LineItems = order.LineItems;
            var payload = new
            {
                idempotency_key = idempotencyKey,
                order = new
                {
                    location_id = _locationId,
                    // Include the local order id as the order.reference_id so Square can correlate
                    reference_id = order.OrderId.ToString(),
                    line_items = LineItems,
                    taxes = order.Taxes
                },
                checkout_options = new
                {
                    redirect_url = redirectUrl
                }
            };

            var json = JsonSerializer.Serialize(payload);
            using var request = new HttpRequestMessage(HttpMethod.Post, "/v2/online-checkout/payment-links");
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            request.Headers.Add("Square-Version", _apiVersion);

            using var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                // Surface Square error details for easier debugging
                throw new InvalidOperationException($"Square CreatePaymentLink failed ({response.StatusCode}): {content}");
            }

            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("payment_link", out var paymentLink))
            {
                if (paymentLink.TryGetProperty("url", out var url))
                {
                    return url.GetString()!;
                }
            }

            throw new InvalidOperationException("Square response did not contain a payment_link.url");
        }
    }
}