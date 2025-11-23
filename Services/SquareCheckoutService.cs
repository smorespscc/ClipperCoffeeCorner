using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ClipperCoffeeCorner.Services
{
    // Simple DTOs used when talking to Square
    public class SquareLineItem
    {
        public string Name { get; set; } = string.Empty;
        // Square expects quantity as a string (e.g. "1", "2")
        public string Quantity { get; set; } = "1";
        // Money in **cents**
        public long BasePriceAmount { get; set; }
        public string BasePriceCurrency { get; set; } = "USD";
    }

    public class SquareTax
    {
        public string Name { get; set; } = string.Empty;
        // Percentage as a string, e.g. "10" = 10%
        public string Percentage { get; set; } = "10";
    }

    public class SquareOrder
    {
        public string IdempotencyKey { get; set; } = Guid.NewGuid().ToString();
        public List<SquareLineItem> LineItems { get; set; } = new();
        public List<SquareTax>? Taxes { get; set; }
    }

    public interface ISquareCheckoutService
    {
        /// <summary>
        /// Creates a Square payment link and returns the checkout URL.
        /// </summary>
        Task<string> CreatePaymentLinkAsync(SquareOrder order, string? redirectUrl = null);
    }

    public class SquareCheckoutService : ISquareCheckoutService
    {
        private readonly HttpClient _httpClient;
        private readonly string _accessToken;
        private readonly string _locationId;
        private readonly string _apiVersion;

        public SquareCheckoutService(IHttpClientFactory httpFactory, IConfiguration config)
        {
            _httpClient = httpFactory.CreateClient("Square");

            _accessToken = config["Square:AccessToken"]
                ?? throw new ArgumentNullException("Square:AccessToken");

            _locationId = config["Square:LocationId"]
                ?? throw new ArgumentNullException("Square:LocationId");

            _apiVersion = config["Square:ApiVersion"] ?? "2023-08-16";
        }

        public async Task<string> CreatePaymentLinkAsync(SquareOrder order, string? redirectUrl = null)
        {
            var idempotencyKey = string.IsNullOrWhiteSpace(order.IdempotencyKey)
                ? Guid.NewGuid().ToString()
                : order.IdempotencyKey;

            var payload = new
            {
                idempotency_key = idempotencyKey,
                order = new
                {
                    location_id = _locationId,
                    line_items = order.LineItems.Select(li => new
                    {
                        name = li.Name,
                        quantity = li.Quantity,
                        base_price_money = new
                        {
                            amount = li.BasePriceAmount,
                            currency = li.BasePriceCurrency
                        }
                    }),
                    taxes = order.Taxes?.Select(t => new
                    {
                        name = t.Name,
                        percentage = t.Percentage
                    })
                },
                checkout_options = new
                {
                    redirect_url = redirectUrl
                }
            };

            var json = JsonSerializer.Serialize(payload);

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "/v2/online-checkout/payment-links");

            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            request.Headers.Add("Square-Version", _apiVersion);

            using var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Square CreatePaymentLink failed ({response.StatusCode}): {content}");
            }

            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("payment_link", out var link) &&
                link.TryGetProperty("url", out var urlElement))
            {
                var url = urlElement.GetString();
                if (!string.IsNullOrEmpty(url))
                {
                    return url;
                }
            }

            throw new InvalidOperationException("Square response did not contain payment_link.url");
        }
    }
}
