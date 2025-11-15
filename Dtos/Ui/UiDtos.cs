using System.Collections.Generic;
using System.Text.Json.Serialization;
using ClipperCoffeeCorner.Dtos.Orders;
using ClipperCoffeeCorner.Dtos.Queue;

namespace ClipperCoffeeCorner.Dtos.Ui
{
    /// <summary>Request for POST /ui/submit coming from the front-end</summary>
    public sealed class UiSubmitRequest
    {
        [JsonPropertyName("draftOrderId")] public string? DraftOrderId { get; set; }
        [JsonPropertyName("customerId")] public string? CustomerId { get; set; }
        /// <summary>Source of submission (Kiosk/Web/Mobile/Staff)</summary>
        [JsonPropertyName("submitSource")] public string SubmitSource { get; set; } = "Web";

        /// <summary>Line items</summary>
        [JsonPropertyName("items")] public List<OrderItemDto> Items { get; set; } = new();

        /// <summary>Note to barista</summary>
        [JsonPropertyName("notes")] public string? Notes { get; set; }

        /// <summary>Payment method hint (Card/Cash/etc.)</summary>
        [JsonPropertyName("paymentMethod")] public string PaymentMethod { get; set; } = "Card";

        /// <summary>Helper to convert to CreateOrderRequest if needed</summary>
        public CreateOrderRequest ToCreateOrderRequest() => new()
        {
            CustomerId = CustomerId,
            Notes = Notes,
            Items = Items
        };
    }

    /// <summary>Response for POST /ui/submit</summary>
    public sealed class UiSubmitResponse
    {
        [JsonPropertyName("orderId")] public string OrderId { get; set; } = string.Empty;
        [JsonPropertyName("status")] public string Status { get; set; } = "Submitted";
        [JsonPropertyName("payment")] public PaymentInfoDto Payment { get; set; } = new();
        [JsonPropertyName("queue")] public QueueStatusDto Queue { get; set; } = new();
        [JsonPropertyName("items")] public List<OrderItemDto> Items { get; set; } = new();
    }

    /// <summary>Payment session summary (for redirect, etc.)</summary>
    public sealed class PaymentInfoDto
    {
        [JsonPropertyName("status")] public string Status { get; set; } = "Pending";
        [JsonPropertyName("paymentUrl")] public string? PaymentUrl { get; set; }
    }
}
