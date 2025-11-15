using System.Collections.Generic;
using System.Text.Json.Serialization;
using ClipperCoffeeCorner.Dtos.Queue;

namespace ClipperCoffeeCorner.Dtos.Orders
{
    /// <summary>Request for POST /orders</summary>
    public sealed class CreateOrderRequest
    {
        /// <summary>Line items (at least one)</summary>
        [JsonPropertyName("items")] public List<OrderItemDto> Items { get; set; } = new();

        /// <summary>Optional note to barista</summary>
        [JsonPropertyName("notes")] public string? Notes { get; set; }

        /// <summary>Optional customer id (for auth or kiosk)</summary>
        [JsonPropertyName("customerId")] public string? CustomerId { get; set; }
    }

    /// <summary>Single line item in an order</summary>
    public sealed class OrderItemDto
    {
        /// <summary>Menu item id (e.g., "mocha", "lotus")</summary>
        [JsonPropertyName("menuItemId")] public string MenuItemId { get; set; } = string.Empty;

        /// <summary>Quantity (>=1)</summary>
        [JsonPropertyName("quantity")] public int Quantity { get; set; } = 1;

        /// <summary>Client-side price (server should re-calc)</summary>
        [JsonPropertyName("price")] public decimal Price { get; set; }

        /// <summary>Style (hot/iced/blended)</summary>
        [JsonPropertyName("style")] public string? Style { get; set; }

        /// <summary>Milk choice (dairy/oat/soy/etc.)</summary>
        [JsonPropertyName("milk")] public string? Milk { get; set; }

        /// <summary>Flavor ids (e.g., vanilla, hazelnut)</summary>
        [JsonPropertyName("flavors")] public List<string>? Flavors { get; set; }

        /// <summary>Add-on ids (e.g., extra-shot)</summary>
        [JsonPropertyName("additions")] public List<string>? Additions { get; set; }

        /// <summary>Restrictions (e.g., sugar-free)</summary>
        [JsonPropertyName("restrictions")] public List<string>? Restrictions { get; set; }

        /// <summary>Extra options bag (future-proof)</summary>
        [JsonPropertyName("options")] public Dictionary<string, string>? Options { get; set; }
    }

    /// <summary>Response for POST /orders</summary>
    public sealed class CreateOrderResponse
    {
        [JsonPropertyName("orderId")] public string OrderId { get; set; } = string.Empty;

        /// <summary>Total in currency units (e.g., USD)</summary>
        [JsonPropertyName("total")] public decimal Total { get; set; }

        [JsonPropertyName("currency")] public string Currency { get; set; } = "USD";

        /// <summary>Payment status (Pending/Paid/Failed)</summary>
        [JsonPropertyName("paymentStatus")] public string PaymentStatus { get; set; } = "Pending";

        /// <summary>Queue status snapshot at creation</summary>
        [JsonPropertyName("queueStatus")] public QueueStatusDto QueueStatus { get; set; } = new();

        /// <summary>Echo of normalized items</summary>
        [JsonPropertyName("items")] public List<OrderItemDto> Items { get; set; } = new();
    }
}
