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

        // Square uses string location ids; keep as string to match Square
        [JsonPropertyName("location_id")]
        public required string LocationId { get; set; }

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

        // method to calculate totals
        public void CalculateTotals()
        {
            SubtotalMoney = LineItems.Sum(li => li.BasePriceMoney.Amount * int.Parse(li.Quantity));
            TotalTaxMoney = Taxes.Sum(t => t.Amount) + ServiceCharges.Sum(sc => sc.Taxes.Sum(t => t.Amount));
            TotalDiscountMoney = Discounts.Sum(d => d.Amount);
            TotalMoney = SubtotalMoney + TotalTaxMoney - TotalDiscountMoney + ServiceCharges.Sum(sc => sc.Amount);
        }

        /// <summary>
        /// Recomputes all totals from line-level data and order-level taxes/discounts/service charges,
        /// and validates them against the stored computed fields. Returns true when values match.
        /// </summary>
        public bool ValidateTotals(out string? error)
            {
            CalculateTotals();
            if (SubtotalMoney != LineItems.Sum(li => li.BasePriceMoney.Amount * int.Parse(li.Quantity)))
            {
                error = "Subtotal money does not match computed line item totals.";
                return false;
            }
            if (TotalTaxMoney != Taxes.Sum(t => t.Amount) + ServiceCharges.Sum(sc => sc.Taxes.Sum(t => t.Amount)))
            {
                error = "Total tax money does not match computed tax totals.";
                return false;
            }
            if (TotalDiscountMoney != Discounts.Sum(d => d.Amount))
            {
                error = "Total discount money does not match computed discount totals.";
                return false;
            }
            if (TotalMoney != SubtotalMoney + TotalTaxMoney - TotalDiscountMoney + ServiceCharges.Sum(sc => sc.Amount))
            {
                error = "Total money does not match computed total.";
                return false;
            }
            error = null;
            return true;
        }
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
        public string Quantity { get; set; } = "1";

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
        [JsonPropertyName("uid")]
        public string Uid { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        // Rate as decimal fraction (0.07 == 7%). Keep for readability; amount is authoritative.
        [JsonPropertyName("rate")]
        public decimal Rate { get; set; }

        // Computed tax amount in minor units (cents)
        [JsonPropertyName("amount")]
        public long Amount { get; set; }
    }

    public sealed class DiscountLine
    {
        [JsonPropertyName("uid")]
        public string Uid { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        // Computed discount amount in minor units (positive = reduction)
        [JsonPropertyName("amount")]
        public long Amount { get; set; }

        // Optional percentage (0.10 == 10%)
        [JsonPropertyName("percentage")]
        public decimal? Percentage { get; set; }
    }
        
    public sealed class ServiceCharge
    {
        [JsonPropertyName("uid")]
        public string Uid { get; set; } = Guid.NewGuid().ToString();

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        // Amount in minor units
        [JsonPropertyName("amount")]
        public long Amount { get; set; }

        [JsonPropertyName("taxes")]
        public List<TaxLine> Taxes { get; set; } = new();
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