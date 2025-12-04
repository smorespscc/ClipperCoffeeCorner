using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ClipperCoffeeCorner.Models.Domain
{
    /// <summary>
    /// Represents a customer's saved favorite order configuration.
    /// Allows customers to save and quickly reorder their favorite combinations.
    /// 
    /// FUTURE DATABASE MAPPING:
    /// This will map to a SavedOrders table with columns:
    /// - Id (PK, int, identity)
    /// - CustomerId (FK to Customers, nvarchar(450))
    /// - Name (nvarchar(100)) - customer's name for this saved order
    /// - ItemName (nvarchar(100)) - the menu item this order is for
    /// - ItemType (nvarchar(20)) - 'Drink' or 'Food'
    /// - Tabs (nvarchar(max)) - JSON array of tab configurations
    /// - SavedAt (datetime2)
    /// - UpdatedAt (datetime2)
    /// 
    /// Related table: SavedOrderTabs
    /// - Id (PK, int, identity)
    /// - SavedOrderId (FK to SavedOrders, int)
    /// - Modifiers (nvarchar(max)) - JSON array
    /// - SpecialRequests (nvarchar(500), nullable)
    /// - TabOrder (int) - for maintaining order
    /// </summary>
    public class SavedOrder
    {
        /// <summary>
        /// Unique identifier for this saved order
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Customer who owns this saved order
        /// </summary>
        [Required]
        [MaxLength(450)]
        public required string CustomerId { get; set; }

        /// <summary>
        /// Customer-provided name for this saved order
        /// (e.g., "My Usual Morning Coffee", "Friday Breakfast")
        /// </summary>
        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }

        /// <summary>
        /// The menu item this saved order is for
        /// </summary>
        [Required]
        [MaxLength(100)]
        public required string ItemName { get; set; }

        /// <summary>
        /// Type of item: "Drink" or "Food"
        /// </summary>
        [Required]
        [MaxLength(20)]
        public required string ItemType { get; set; }

        /// <summary>
        /// Array of tab configurations (each tab represents one instance of the item)
        /// Each tab contains modifiers and special requests
        /// Example: User wants 2 cappuccinos with different configurations
        /// </summary>
        [JsonPropertyName("tabs")]
        public List<SavedOrderTab> Tabs { get; set; } = new List<SavedOrderTab>();

        /// <summary>
        /// Timestamp when order was first saved
        /// </summary>
        public DateTime SavedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp when order was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties for EF Core (future use)
        // public virtual Customer? Customer { get; set; }

        /// <summary>
        /// Gets total quantity (number of tabs/instances)
        /// </summary>
        public int GetTotalQuantity()
        {
            return Tabs?.Count ?? 0;
        }

        /// <summary>
        /// Converts this saved order to a list of OrderItems for adding to cart
        /// </summary>
        public List<OrderItem> ToOrderItems(decimal basePrice, Dictionary<string, decimal> modifierPrices)
        {
            var orderItems = new List<OrderItem>();

            foreach (var tab in Tabs)
            {
                var orderItem = new OrderItem
                {
                    Name = ItemName,
                    Type = ItemType,
                    BasePrice = basePrice,
                    Quantity = 1,
                    Modifiers = new List<string>(tab.Modifiers),
                    SpecialRequests = tab.SpecialRequests,
                    FromSavedOrderId = Id
                };

                orderItem.CalculateUnitPrice(modifierPrices);
                orderItem.CalculateTotalPrice();

                orderItems.Add(orderItem);
            }

            return orderItems;
        }
    }

    /// <summary>
    /// Represents a single tab/instance configuration within a saved order.
    /// Each tab is one instance of the item with specific modifiers and requests.
    /// </summary>
    public class SavedOrderTab
    {
        /// <summary>
        /// Unique identifier for this tab (for database)
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Reference to parent saved order
        /// </summary>
        public int SavedOrderId { get; set; }

        /// <summary>
        /// Selected modifiers for this instance
        /// </summary>
        [JsonPropertyName("modifiers")]
        public List<string> Modifiers { get; set; } = new List<string>();

        /// <summary>
        /// Special requests for this instance
        /// </summary>
        [MaxLength(500)]
        [JsonPropertyName("specialRequests")]
        public string? SpecialRequests { get; set; }

        /// <summary>
        /// Order of this tab (for maintaining sequence)
        /// </summary>
        public int TabOrder { get; set; }

        // Navigation properties for EF Core (future use)
        // public virtual SavedOrder? SavedOrder { get; set; }
    }
}
