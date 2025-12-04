using ClipperCoffeeCorner.Models.Domain;
using System.Collections.Generic;
using System.Linq;

namespace ClipperCoffeeCorner.Models.DTOs
{
    /// <summary>
    /// Data Transfer Object for cart operations.
    /// Used for API responses when cart is modified.
    /// Contains cart items and calculated totals.
    /// 
    /// This DTO is returned by OrderController API endpoints and consumed by JavaScript.
    /// </summary>
    public class CartDto
    {
        /// <summary>
        /// List of items currently in cart
        /// </summary>
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        /// <summary>
        /// Total number of items (sum of quantities)
        /// </summary>
        public int TotalItemCount => Items.Sum(item => item.Quantity);

        /// <summary>
        /// Number of unique items (distinct entries)
        /// </summary>
        public int UniqueItemCount => Items.Count;

        /// <summary>
        /// Subtotal before discounts and tax
        /// </summary>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Staff discount amount (if applicable)
        /// </summary>
        public decimal StaffDiscount { get; set; }

        /// <summary>
        /// Total after discounts
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// Whether user is staff (eligible for discount)
        /// </summary>
        public bool IsStaff { get; set; }

        /// <summary>
        /// Staff discount percentage (e.g., 0.10 for 10%)
        /// </summary>
        public decimal StaffDiscountPercentage { get; set; } = 0.10m;

        /// <summary>
        /// Whether cart is empty
        /// </summary>
        public bool IsEmpty => Items.Count == 0;

        /// <summary>
        /// Whether cart has items
        /// </summary>
        public bool HasItems => Items.Count > 0;

        /// <summary>
        /// Global special requests for entire order
        /// </summary>
        public string? SpecialRequests { get; set; }

        /// <summary>
        /// Session ID this cart belongs to
        /// </summary>
        public string? SessionId { get; set; }

        /// <summary>
        /// Timestamp of last cart modification
        /// </summary>
        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Success message for UI display
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Whether the operation was successful
        /// </summary>
        public bool Success { get; set; } = true;

        // ==================== HELPER METHODS ====================

        /// <summary>
        /// Calculates all totals based on items
        /// </summary>
        public void CalculateTotals(bool isStaff = false)
        {
            Subtotal = Items.Sum(item => item.TotalPrice);
            IsStaff = isStaff;

            if (isStaff)
            {
                StaffDiscount = Subtotal * StaffDiscountPercentage;
                Total = Subtotal - StaffDiscount;
            }
            else
            {
                StaffDiscount = 0;
                Total = Subtotal;
            }
        }

        /// <summary>
        /// Gets formatted subtotal for display
        /// </summary>
        public string GetFormattedSubtotal() => $"${Subtotal:F2}";

        /// <summary>
        /// Gets formatted staff discount for display
        /// </summary>
        public string GetFormattedStaffDiscount() => $"-${StaffDiscount:F2}";

        /// <summary>
        /// Gets formatted total for display
        /// </summary>
        public string GetFormattedTotal() => $"${Total:F2}";

        /// <summary>
        /// Creates a summary string of cart contents
        /// </summary>
        public string GetCartSummary()
        {
            if (IsEmpty) return "Cart is empty";

            var itemNames = Items.Select(i => $"{i.Name} x{i.Quantity}");
            return string.Join(", ", itemNames);
        }

        /// <summary>
        /// Finds an item in cart by index
        /// </summary>
        public OrderItem? GetItemByIndex(int index)
        {
            if (index < 0 || index >= Items.Count) return null;
            return Items[index];
        }

        /// <summary>
        /// Finds all items matching name and type
        /// </summary>
        public List<OrderItem> GetItemsByNameAndType(string name, string type)
        {
            return Items.Where(i => i.Name == name && i.Type == type).ToList();
        }

        /// <summary>
        /// Checks if cart contains a specific item
        /// </summary>
        public bool ContainsItem(string name, string type)
        {
            return Items.Any(i => i.Name == name && i.Type == type);
        }

        /// <summary>
        /// Gets total quantity for a specific item
        /// </summary>
        public int GetItemQuantity(string name, string type)
        {
            return Items
                .Where(i => i.Name == name && i.Type == type)
                .Sum(i => i.Quantity);
        }
    }
}
