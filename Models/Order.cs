using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace ClipperCoffeeCorner.Models
{
    /// <summary>
    /// All monetary values are stored as integer minor-units (e.g., cents).
    /// JSON property names use snake_case to align with API payload conventions.
    /// WORK IN PROGRESS: this is an initial version and may evolve over time.
    /// </summary>
    public sealed class Order
    {
        // Canonical order id in our system (UUID)
        [JsonPropertyName("order_id")]
        public int OrderId { get; set; }

        // Client-provided idempotency key - must be persisted with the order
        [JsonPropertyName("idempotency_key")]
        public required string IdempotencyKey { get; set; }

        // Customer id (optional but recommended)
        [JsonPropertyName("customer_id")]
        public string? CustomerId { get; set; }

        // ISO currency code (e.g. "USD")
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "USD";

        // Line items (Square calls these line_items)
        [JsonPropertyName("line_items")]
        public List<LineItem> LineItems { get; set; } = new();

        // Order-level taxes (e.g., state sales tax)
        [JsonPropertyName("taxes")]
        public List<TaxLine> Taxes { get; set; } = new();

        // Order-level discounts (if any)
        [JsonPropertyName("discounts")]
        public List<DiscountLine> Discounts { get; set; } = new();

        // Service charges (Square support)
        [JsonPropertyName("service_charges")]
        public List<ServiceCharge> ServiceCharges { get; set; } = new();

        // Computed money fields (minor units)
        [JsonPropertyName("subtotal_money")]
        public long SubtotalMoney { get; set; }

        [JsonPropertyName("total_tax_money")]
        public long TotalTaxMoney { get; set; }

        [JsonPropertyName("total_discount_money")]
        public long TotalDiscountMoney { get; set; }

        [JsonPropertyName("total_money")]
        public long TotalMoney { get; set; }

        // Timestamps (RFC3339 / DateTimeOffset)
        [JsonPropertyName("created_at")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [JsonPropertyName("placed_at")]
        public DateTimeOffset? PlacedAt { get; set; }

        [JsonPropertyName("completed_at")]
        public DateTimeOffset? CompletedAt { get; set; }

        // Record of alterations for audit
        [JsonPropertyName("alterations")]
        public List<OrderAlteration>? Alterations { get; set; }

        // Order status - common states used with payment link flows
        [JsonPropertyName("status")]
        public OrderStatus Status { get; set; } = OrderStatus.Open;
    }

    public sealed class LineItem
    {
        // product / catalog id or SKU
        [JsonPropertyName("catalog_object_id")]
        public string? CatalogObjectId { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        // Unit price in minor units (e.g., cents)
        [JsonPropertyName("base_price_money")]
        public required Money BasePriceMoney { get; set; }

        // Quantity as integer (Square sometimes models as string; using int simplifies computations)
        [JsonPropertyName("quantity")]
        public required string Quantity { get; set; }

        // Item-level taxes (computed amounts in minor units)
        // currently don't expect to use item-level taxes, so commented out for now
        /*
        [JsonPropertyName("taxes")]
        public List<TaxLine>? Taxes { get; set; }
        */

        // Item-level discounts (computed amounts in minor units)
        // currently don't expect to use item-level discounts, so commented out for now
        /* 
        [JsonPropertyName("discounts")]
        public List<DiscountLine>? Discounts { get; set; }
        */

        // Line total in minor units for auditability: (unit * qty) - discounts + item taxes
        // currently don't expect to use item-level taxes/discounts, so just unit * qty
        /*
        [JsonPropertyName("line_total_money")]
        public long LineTotalMoney { get; set; }
        */
    }

    public sealed class TaxLine
    {
        [JsonPropertyName("OrderId")]
        public string? OrderId { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = "ADDITIVE";

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        // Rate as decimal fraction (3.6 = 3.6%). Keep for readability; amount is authoritative.
        [JsonPropertyName("percentage")]
        public required string Percentage { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; } = "ORDER";

        // Computed tax amount in minor units (cents)
        // amount is calcuated on payment link creation
        /*
        [JsonPropertyName("amount")]
        public long Amount { get; set; }
        */
    }

    public sealed class DiscountLine
    {
        [JsonPropertyName("OrderId")]
        public string? OrderId { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        // Computed discount amount in minor units (positive = reduction)
        // amount is calcuated on payment link creation
        /*
        [JsonPropertyName("amount")]
        public long Amount { get; set; }
        */

        // Optional percentage (3.6 = 3.6%)
        [JsonPropertyName("percentage")]
        public decimal Percentage { get; set; }
    }
        
    public sealed class ServiceCharge
    {
        [JsonPropertyName("OrderId")]
        public string? OrderId { get; set; }

        [JsonPropertyName("name")]
        public required string Name { get; set; }

        // Amount in minor units
        // amount is calcuated on payment link creation
        /*
        [JsonPropertyName("amount")]
        public long Amount { get; set; }
        */

        // Optional percentage (3.6 = 3.6%)
        [JsonPropertyName("percentage")]
        public required string Percentage { get; set; }

        [JsonPropertyName("taxable")]
        public bool Taxable { get; set; }
    }

    public sealed class OrderAlteration
    {
        [JsonPropertyName("timestamp")]
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("changed_by")]
        public string? ChangedBy { get; set; }
    }

    public sealed class Money
    {
        [JsonPropertyName("amount")]
        public long Amount { get; set; }
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "USD";
    }

    public enum OrderStatus
    {
        // Common states used in payment link / order lifecycles
        Open,
        Placed,
        Completed,
        Cancelled,
        Draft
    }
}