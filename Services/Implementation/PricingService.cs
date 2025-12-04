using ClipperCoffeeCorner.Models.Domain;
using ClipperCoffeeCorner.Services.Configuration;
using ClipperCoffeeCorner.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClipperCoffeeCorner.Services.Implementation
{
    /// <summary>
    /// Implementation of pricing calculations and discount logic.
    /// This service handles all money-related calculations in the application.
    /// 
    /// BUSINESS RULES:
    /// - Base prices defined in MenuConfiguration
    /// - Modifiers can add cost (e.g., Extra Shot +$0.75)
    /// - Staff members receive 10% discount on subtotal
    /// - All prices rounded to 2 decimal places
    /// 
    /// This is a stateless service - all calculations are pure functions.
    /// </summary>
    public class PricingService : IPricingService
    {
        // ==================== ITEM PRICING ====================
        
        /// <summary>
        /// Gets base price for a menu item
        /// </summary>
        public Task<decimal> GetBasePriceAsync(string itemName)
        {
            var price = MenuConfiguration.GetBasePrice(itemName);
            return Task.FromResult(price);
        }

        /// <summary>
        /// Calculates price for an item with modifiers applied
        /// </summary>
        public Task<decimal> CalculateItemPriceAsync(string itemName, List<string> modifiers)
        {
            var price = MenuConfiguration.CalculatePriceWithModifiers(itemName, modifiers);
            return Task.FromResult(price);
        }

        /// <summary>
        /// Calculates price for an OrderItem (base + modifiers * quantity)
        /// </summary>
        public Task<decimal> CalculateOrderItemPriceAsync(OrderItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            // Get base price
            var basePrice = MenuConfiguration.GetBasePrice(item.Name);
            
            // Add modifier costs
            var modifierCost = 0m;
            if (item.Modifiers != null && item.Modifiers.Count > 0)
            {
                foreach (var modifier in item.Modifiers)
                {
                    modifierCost += MenuConfiguration.GetModifierPrice(modifier);
                }
            }
            
            // Calculate unit price and total
            var unitPrice = basePrice + modifierCost;
            var totalPrice = unitPrice * item.Quantity;
            
            return Task.FromResult(totalPrice);
        }

        // ==================== MODIFIER PRICING ====================
        
        /// <summary>
        /// Gets price adjustment for a specific modifier
        /// </summary>
        public Task<decimal> GetModifierPriceAsync(string modifierName)
        {
            var price = MenuConfiguration.GetModifierPrice(modifierName);
            return Task.FromResult(price);
        }

        /// <summary>
        /// Gets all modifier prices as a dictionary
        /// </summary>
        public Task<Dictionary<string, decimal>> GetAllModifierPricesAsync()
        {
            var prices = new Dictionary<string, decimal>(MenuConfiguration.ModifierPrices);
            return Task.FromResult(prices);
        }

        /// <summary>
        /// Calculates total cost of selected modifiers
        /// </summary>
        public Task<decimal> CalculateModifiersCostAsync(List<string> modifiers)
        {
            if (modifiers == null || modifiers.Count == 0)
                return Task.FromResult(0m);

            var totalCost = modifiers.Sum(modifier => MenuConfiguration.GetModifierPrice(modifier));
            return Task.FromResult(totalCost);
        }

        // ==================== ORDER TOTALS ====================
        
        /// <summary>
        /// Calculates subtotal for a list of order items (before discounts)
        /// </summary>
        public async Task<decimal> CalculateSubtotalAsync(List<OrderItem> items)
        {
            if (items == null || items.Count == 0)
                return 0m;

            decimal subtotal = 0m;
            
            foreach (var item in items)
            {
                var itemPrice = await CalculateOrderItemPriceAsync(item);
                subtotal += itemPrice;
            }
            
            return Math.Round(subtotal, 2);
        }

        /// <summary>
        /// Calculates staff discount amount (10% of subtotal)
        /// </summary>
        public Task<decimal> CalculateStaffDiscountAsync(decimal subtotal, bool isStaff)
        {
            if (!isStaff || subtotal <= 0)
                return Task.FromResult(0m);

            var discount = subtotal * MenuConfiguration.StaffDiscountPercentage;
            return Task.FromResult(Math.Round(discount, 2));
        }

        /// <summary>
        /// Calculates final total with all discounts applied
        /// </summary>
        public async Task<decimal> CalculateTotalAsync(List<OrderItem> items, bool isStaff)
        {
            var subtotal = await CalculateSubtotalAsync(items);
            var discount = await CalculateStaffDiscountAsync(subtotal, isStaff);
            var total = subtotal - discount;
            
            return Math.Round(total, 2);
        }

        /// <summary>
        /// Calculates all order totals at once (subtotal, discount, total)
        /// </summary>
        public async Task<(decimal subtotal, decimal discount, decimal total)> CalculateOrderTotalsAsync(
            List<OrderItem> items, 
            bool isStaff)
        {
            var subtotal = await CalculateSubtotalAsync(items);
            var discount = await CalculateStaffDiscountAsync(subtotal, isStaff);
            var total = subtotal - discount;
            
            return (
                Math.Round(subtotal, 2),
                Math.Round(discount, 2),
                Math.Round(total, 2)
            );
        }

        // ==================== VALIDATION ====================
        
        /// <summary>
        /// Validates that a price is within acceptable range
        /// </summary>
        public Task<bool> ValidatePriceAsync(decimal price)
        {
            // Price must be positive and less than $1000
            var isValid = price > 0 && price < 1000m;
            return Task.FromResult(isValid);
        }

        /// <summary>
        /// Validates that an order total is reasonable
        /// </summary>
        public Task<bool> ValidateOrderTotalAsync(decimal total, int itemCount)
        {
            // Basic sanity checks
            if (total <= 0) return Task.FromResult(false);
            if (itemCount <= 0) return Task.FromResult(false);
            
            // Average price per item should be between $1 and $50
            var avgPricePerItem = total / itemCount;
            var isValid = avgPricePerItem >= 1m && avgPricePerItem <= 50m;
            
            return Task.FromResult(isValid);
        }

        // ==================== FORMATTING ====================
        
        /// <summary>
        /// Formats a price for display (e.g., "$4.50")
        /// </summary>
        public Task<string> FormatPriceAsync(decimal price)
        {
            var formatted = $"${price:F2}";
            return Task.FromResult(formatted);
        }

        /// <summary>
        /// Formats a discount for display (e.g., "-$0.45")
        /// </summary>
        public Task<string> FormatDiscountAsync(decimal discount)
        {
            var formatted = discount > 0 ? $"-${discount:F2}" : "$0.00";
            return Task.FromResult(formatted);
        }

        // ==================== HELPER METHODS ====================
        
        /// <summary>
        /// Updates an OrderItem with calculated prices
        /// This modifies the item in place
        /// </summary>
        public async Task UpdateOrderItemPricesAsync(OrderItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            // Get base price
            item.BasePrice = await GetBasePriceAsync(item.Name);
            
            // Calculate unit price with modifiers
            var modifierCost = await CalculateModifiersCostAsync(item.Modifiers);
            item.UnitPrice = item.BasePrice + modifierCost;
            
            // Calculate total price
            item.TotalPrice = item.UnitPrice * item.Quantity;
        }

        /// <summary>
        /// Updates all items in a list with calculated prices
        /// </summary>
        public async Task UpdateAllOrderItemPricesAsync(List<OrderItem> items)
        {
            if (items == null || items.Count == 0)
                return;

            foreach (var item in items)
            {
                await UpdateOrderItemPricesAsync(item);
            }
        }

        /// <summary>
        /// Gets a breakdown of costs for an order item
        /// Useful for displaying itemized pricing
        /// </summary>
        public async Task<PriceBreakdown> GetPriceBreakdownAsync(OrderItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var breakdown = new PriceBreakdown
            {
                ItemName = item.Name,
                BasePrice = await GetBasePriceAsync(item.Name),
                ModifierCosts = new Dictionary<string, decimal>(),
                Quantity = item.Quantity
            };

            // Add each modifier cost
            if (item.Modifiers != null && item.Modifiers.Count > 0)
            {
                foreach (var modifier in item.Modifiers)
                {
                    var cost = await GetModifierPriceAsync(modifier);
                    if (cost > 0)
                    {
                        breakdown.ModifierCosts[modifier] = cost;
                    }
                }
            }

            breakdown.TotalModifierCost = breakdown.ModifierCosts.Values.Sum();
            breakdown.UnitPrice = breakdown.BasePrice + breakdown.TotalModifierCost;
            breakdown.TotalPrice = breakdown.UnitPrice * breakdown.Quantity;

            return breakdown;
        }
    }
}
