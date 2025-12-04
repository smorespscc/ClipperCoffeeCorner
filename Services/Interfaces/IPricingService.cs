using ClipperCoffeeCorner.Models.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClipperCoffeeCorner.Services.Interfaces
{
    /// <summary>
    /// Business logic for pricing calculations and discount logic.
    /// Handles all money-related calculations in the application.
    /// </summary>
    public interface IPricingService
    {
        // ==================== ITEM PRICING ====================
        
        /// <summary>
        /// Gets base price for a menu item
        /// </summary>
        Task<decimal> GetBasePriceAsync(string itemName);
        
        /// <summary>
        /// Calculates price for an item with modifiers applied
        /// </summary>
        Task<decimal> CalculateItemPriceAsync(string itemName, List<string> modifiers);
        
        /// <summary>
        /// Calculates price for an OrderItem (base + modifiers * quantity)
        /// </summary>
        Task<decimal> CalculateOrderItemPriceAsync(OrderItem item);

        // ==================== MODIFIER PRICING ====================
        
        /// <summary>
        /// Gets price adjustment for a specific modifier
        /// </summary>
        Task<decimal> GetModifierPriceAsync(string modifierName);
        
        /// <summary>
        /// Gets all modifier prices as a dictionary
        /// </summary>
        Task<Dictionary<string, decimal>> GetAllModifierPricesAsync();
        
        /// <summary>
        /// Calculates total cost of selected modifiers
        /// </summary>
        Task<decimal> CalculateModifiersCostAsync(List<string> modifiers);

        // ==================== ORDER TOTALS ====================
        
        /// <summary>
        /// Calculates subtotal for a list of order items (before discounts)
        /// </summary>
        Task<decimal> CalculateSubtotalAsync(List<OrderItem> items);
        
        /// <summary>
        /// Calculates staff discount amount (10% of subtotal)
        /// </summary>
        Task<decimal> CalculateStaffDiscountAsync(decimal subtotal, bool isStaff);
        
        /// <summary>
        /// Calculates final total with all discounts applied
        /// </summary>
        Task<decimal> CalculateTotalAsync(List<OrderItem> items, bool isStaff);
        
        /// <summary>
        /// Calculates all order totals at once (subtotal, discount, total)
        /// </summary>
        Task<(decimal subtotal, decimal discount, decimal total)> CalculateOrderTotalsAsync(
            List<OrderItem> items, 
            bool isStaff);

        // ==================== VALIDATION ====================
        
        /// <summary>
        /// Validates that a price is within acceptable range
        /// </summary>
        Task<bool> ValidatePriceAsync(decimal price);
        
        /// <summary>
        /// Validates that an order total is reasonable
        /// </summary>
        Task<bool> ValidateOrderTotalAsync(decimal total, int itemCount);

        // ==================== FORMATTING ====================
        
        /// <summary>
        /// Formats a price for display (e.g., "$4.50")
        /// </summary>
        Task<string> FormatPriceAsync(decimal price);
        
        /// <summary>
        /// Formats a discount for display (e.g., "-$0.45")
        /// </summary>
        Task<string> FormatDiscountAsync(decimal discount);

        // ==================== HELPER METHODS ====================
        
        /// <summary>
        /// Updates an OrderItem with calculated prices
        /// </summary>
        Task UpdateOrderItemPricesAsync(OrderItem item);
        
        /// <summary>
        /// Updates all items in a list with calculated prices
        /// </summary>
        Task UpdateAllOrderItemPricesAsync(List<OrderItem> items);
        
        /// <summary>
        /// Gets a breakdown of costs for an order item
        /// </summary>
        Task<PriceBreakdown> GetPriceBreakdownAsync(OrderItem item);
    }

    /// <summary>
    /// Represents a detailed breakdown of pricing for an order item
    /// </summary>
    public class PriceBreakdown
    {
        public string ItemName { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public Dictionary<string, decimal> ModifierCosts { get; set; } = new Dictionary<string, decimal>();
        public decimal TotalModifierCost { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// Gets a formatted string representation of the breakdown
        /// </summary>
        public string GetFormattedBreakdown()
        {
            var lines = new List<string>
            {
                $"{ItemName}:",
                $"  Base Price: ${BasePrice:F2}"
            };

            if (ModifierCosts.Count > 0)
            {
                lines.Add("  Modifiers:");
                foreach (var (modifier, cost) in ModifierCosts)
                {
                    lines.Add($"    {modifier}: +${cost:F2}");
                }
            }

            lines.Add($"  Unit Price: ${UnitPrice:F2}");
            lines.Add($"  Quantity: {Quantity}");
            lines.Add($"  Total: ${TotalPrice:F2}");

            return string.Join(System.Environment.NewLine, lines);
        }
    }
}
