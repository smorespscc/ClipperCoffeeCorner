using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ClipperCoffeeCorner.Models.Domain
{
    /// <summary>
    /// Represents a single item in an order or cart.
    /// Includes the base menu item plus customer customizations (modifiers, special requests).
    /// 
    /// FUTURE DATABASE MAPPING:
    /// This will map to an OrderItems table with columns:
    /// - Id (PK, int, identity)
    /// - OrderId (FK to Orders, int, nullable for cart items)
    /// - SessionId (nvarchar(100), nullable - for cart items before order creation)
    /// - MenuItemId (FK to MenuItems, int)
    /// - ItemName (nvarchar(100)) - denormalized for historical accuracy
    /// - ItemType (nvarchar(20)) - 'Drink' or 'Food'
    /// - BasePrice (decimal(10,2)) - denormalized for historical accuracy
    /// - Quantity (int)
    /// - Modifiers (nvarchar(max)) - JSON array of selected modifiers
    /// - SpecialRequests (nvarchar(500), nullable)
    /// - UnitPrice (decimal(10,2)) - base price + modifier adjustments
    /// - TotalPrice (decimal(10,2)) - unit price * quantity
    /// - FromSavedOrderId (int, nullable) - tracks if item came from a saved order
    /// - CreatedAt (datetime2)
    /// </summary>
    public class OrderItem
    {
        /// <summary>
        /// Unique identifier for this order item
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Reference to parent order (null for cart items)
        /// </summary>
        public int? OrderId { get; set; }

        /// <summary>
        /// Session ID for cart items (before order is created)
        /// </summary>
        [MaxLength(100)]
        public string? SessionId { get; set; }

        /// <summary>
        /// Reference to menu item
        /// </summary>
        public int? MenuItemId { get; set; }

        /// <summary>
        /// Name of the item (denormalized for historical accuracy)
        /// Even if menu item is deleted/renamed, order history remains accurate
        /// </summary>
        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }

        /// <summary>
        /// Type of item: "Drink" or "Food"
        /// </summary>
        [Required]
        [MaxLength(20)]
        public required string Type { get; set; }

        /// <summary>
        /// Base price at time of order (denormalized)
        /// </summary>
        [Required]
        [Range(0.01, 999.99)]
        public decimal BasePrice { get; set; }

        /// <summary>
        /// Quantity of this item
        /// </summary>
        [Required]
        [Range(1, 99)]
        public int Quantity { get; set; } = 1;

        /// <summary>
        /// Selected modifiers (e.g., ["Hot", "Oat Milk", "Extra Shot"])
        /// Stored as JSON array for flexibility
        /// </summary>
        [JsonPropertyName("modifiers")]
        public List<string> Modifiers { get; set; } = new List<string>();

        /// <summary>
        /// Customer's special requests or notes
        /// </summary>
        [MaxLength(500)]
        [JsonPropertyName("specialRequests")]
        public string? SpecialRequests { get; set; }

        /// <summary>
        /// Unit price including modifier adjustments
        /// Calculated as: BasePrice + sum of modifier prices
        /// </summary>
        [Range(0.01, 999.99)]
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Total price for this line item
        /// Calculated as: UnitPrice * Quantity
        /// </summary>
        [Range(0.01, 9999.99)]
        public decimal TotalPrice { get; set; }

        /// <summary>
        /// If this item came from a saved order, track which one
        /// Used to manage "applied saved orders" feature
        /// </summary>
        public int? FromSavedOrderId { get; set; }

        /// <summary>
        /// Timestamp when item was added
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties for EF Core (future use)
        // public virtual Order? Order { get; set; }
        // public virtual MenuItem? MenuItem { get; set; }
        // public virtual SavedOrder? FromSavedOrder { get; set; }

        /// <summary>
        /// Helper method to calculate unit price based on modifiers
        /// </summary>
        public void CalculateUnitPrice(Dictionary<string, decimal> modifierPrices)
        {
            UnitPrice = BasePrice;
            
            if (Modifiers != null && Modifiers.Count > 0)
            {
                foreach (var modifier in Modifiers)
                {
                    if (modifierPrices.TryGetValue(modifier, out decimal modifierPrice))
                    {
                        UnitPrice += modifierPrice;
                    }
                }
            }
        }

        /// <summary>
        /// Helper method to calculate total price
        /// </summary>
        public void CalculateTotalPrice()
        {
            TotalPrice = UnitPrice * Quantity;
        }

        /// <summary>
        /// Creates a deep copy of this order item
        /// </summary>
        public OrderItem Clone()
        {
            return new OrderItem
            {
                Name = this.Name,
                Type = this.Type,
                BasePrice = this.BasePrice,
                Quantity = this.Quantity,
                Modifiers = new List<string>(this.Modifiers),
                SpecialRequests = this.SpecialRequests,
                UnitPrice = this.UnitPrice,
                TotalPrice = this.TotalPrice,
                FromSavedOrderId = this.FromSavedOrderId
            };
        }
    }
}
