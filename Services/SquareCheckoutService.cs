using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using ClipperCoffeeCorner.Models;
using ClipperCoffeeCorner.Data;

namespace ClipperCoffeeCorner.Services
{
    public class SquareCheckoutService : ISquareCheckoutService
    {
        private readonly HttpClient _httpClient;
        private readonly string _accessToken;
        private readonly string _locationId;
        private readonly string _apiVersion;
        private readonly AppDbContext _db;

        public SquareCheckoutService(IHttpClientFactory httpFactory, IConfiguration config, AppDbContext dbContext)
    {
            _httpClient = httpFactory.CreateClient("Square");
            _accessToken = config["Square:AccessToken"] ?? throw new ArgumentNullException("Square:AccessToken");
            _locationId = config["Square:LocationId"] ?? throw new ArgumentNullException("Square:LocationId");
            _apiVersion = config["Square:ApiVersion"] ?? "2023-08-16";
            _db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

        public async Task<string> CreatePaymentLinkAsync(Order order, string? redirectUrl = null)
    {
            if (order is null) throw new ArgumentNullException(nameof(order));

            // Load authoritative order from the database (including items + combination metadata)
            var dbOrder = await _db.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems!)
                    .ThenInclude(oi => oi.Combination)
                .FirstOrDefaultAsync(o => o.OrderId == order.OrderId);

            if (dbOrder is null)
    {
                throw new InvalidOperationException($"Order with id {order.OrderId} not found in database.");
    }

            var idempotencyKey = dbOrder.IdempotencyKey;

            // Map DB OrderItems to Square's expected line_items shape.
            // Square expects quantity as a string and amounts as the smallest currency unit (e.g., cents).
            var lineItems = dbOrder.OrderItems.Select(oi => new
        {
                name = $"Item #{oi.CombinationId}",
                quantity = oi.Quantity.ToString(),
                base_price_money = new
                {
                    amount = Convert.ToInt64(Math.Round(oi.UnitPrice * 100m)), // decimal dollars -> long cents
                    currency = "USD"
        }
            }).ToArray();

            var taxUid = Guid.NewGuid().ToString();
            var taxes = new[]
            {
                new
        {
                    uid = taxUid,
                    name = "Sales Tax",
                    percentage = "9.8",
                    scope = "ORDER",
                    type = "ADDITIVE"
                }
            };

            var payload = new
            {
                idempotency_key = idempotencyKey,
                order = new
                {
                    location_id = _locationId,
                    // Include the local order id as the order.reference_id so Square can correlate
                    reference_id = dbOrder.OrderId.ToString(),
                    line_items = lineItems,
                    taxes
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
