using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ClipperCoffeeCorner.Models.Domain
{
    /// <summary>
    /// Represents a menu item in the cafe.
    /// Used for both drinks and food items.
    /// 
    /// FUTURE DATABASE MAPPING:
    /// This will map to a MenuItems table with columns:
    /// - Id (PK, int, identity)
    /// - Name (nvarchar(100), unique)
    /// - Type (nvarchar(20)) - 'Drink' or 'Food'
    /// - BasePrice (decimal(10,2))
    /// - Description (nvarchar(500), nullable)
    /// - ImageUrl (nvarchar(500), nullable)
    /// - IsAvailable (bit)
    /// - AvailableDuringBreakfast (bit)
    /// - AvailableDuringLunch (bit)
    /// - IsTrending (bit)
    /// - IsSpecial (bit)
    /// - CreatedAt (datetime2)
    /// - UpdatedAt (datetime2)
    /// </summary>
    public class MenuItem
    {
        /// <summary>
        /// Unique identifier for the menu item
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Name of the menu item (e.g., "Cappuccino", "Pancake Stack")
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
        /// Base price before modifiers (in dollars)
        /// </summary>
        [Required]
        [Range(0.01, 999.99)]
        public decimal BasePrice { get; set; }

        /// <summary>
        /// Optional description of the item
        /// </summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>
        /// URL to item image
        /// </summary>
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Whether item is currently available for ordering
        /// </summary>
        public bool IsAvailable { get; set; } = true;

        /// <summary>
        /// Whether item is available during breakfast hours (8:00 AM - 10:30 AM)
        /// </summary>
        public bool AvailableDuringBreakfast { get; set; }

        /// <summary>
        /// Whether item is available during lunch hours (10:30 AM - 2:00 PM)
        /// </summary>
        public bool AvailableDuringLunch { get; set; }

        /// <summary>
        /// Whether item should appear in "Trending" section
        /// </summary>
        public bool IsTrending { get; set; }

        /// <summary>
        /// Whether item should appear in "Specials" section
        /// </summary>
        public bool IsSpecial { get; set; }

        /// <summary>
        /// Available modifiers for this item (e.g., "Hot", "Iced", "Extra Shot")
        /// Stored as comma-separated string for simplicity
        /// FUTURE: Consider separate ModifierOptions table with many-to-many relationship
        /// </summary>
        [MaxLength(1000)]
        public string? AvailableModifiers { get; set; }

        /// <summary>
        /// Timestamp when item was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp when item was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties for EF Core (future use)
        // public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
