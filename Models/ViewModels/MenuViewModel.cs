using ClipperCoffeeCorner.Models.Domain;
using ClipperCoffeeCorner.Services.Interfaces;
using System;
using System.Collections.Generic;

namespace ClipperCoffeeCorner.Models.ViewModels
{
    /// <summary>
    /// ViewModel for the Menu page.
    /// Contains all data needed to render the menu, including:
    /// - Available menu items
    /// - Current service period
    /// - User's cart summary
    /// - Recent and saved orders
    /// 
    /// This ViewModel is populated by the MenuController and passed to the Menu view.
    /// </summary>
    public class MenuViewModel
    {
        // ==================== SERVICE PERIOD INFO ====================
        
        /// <summary>
        /// Current service period (Breakfast, Lunch, or Closed)
        /// </summary>
        public ServicePeriod CurrentServicePeriod { get; set; }

        /// <summary>
        /// Display name for current service period
        /// </summary>
        public string ServicePeriodName => CurrentServicePeriod.ToString();

        /// <summary>
        /// Service hours for current period (e.g., "8:00am - 10:30am")
        /// </summary>
        public string ServiceHours { get; set; } = string.Empty;

        /// <summary>
        /// Icon class for current service period
        /// </summary>
        public string ServiceIcon => CurrentServicePeriod switch
        {
            ServicePeriod.Breakfast => "bi-sunrise-fill",
            ServicePeriod.Lunch => "bi-sun-fill",
            _ => "bi-moon-fill"
        };

        /// <summary>
        /// Whether cafe is currently open
        /// </summary>
        public bool IsOpen => CurrentServicePeriod != ServicePeriod.Closed;

        /// <summary>
        /// Whether user is in viewing mode (viewing menu while closed)
        /// </summary>
        public bool IsViewingMode { get; set; }

        /// <summary>
        /// Whether debug mode is enabled (all items available)
        /// </summary>
        public bool IsDebugMode { get; set; }

        // ==================== MENU ITEMS ====================
        
        /// <summary>
        /// All menu items (filtered by availability if not in debug mode)
        /// </summary>
        public List<MenuItem> AllMenuItems { get; set; } = new List<MenuItem>();

        /// <summary>
        /// Drink menu items
        /// </summary>
        public List<MenuItem> DrinkItems { get; set; } = new List<MenuItem>();

        /// <summary>
        /// Food menu items
        /// </summary>
        public List<MenuItem> FoodItems { get; set; } = new List<MenuItem>();

        /// <summary>
        /// Trending items
        /// </summary>
        public List<MenuItem> TrendingItems { get; set; } = new List<MenuItem>();

        /// <summary>
        /// Special items
        /// </summary>
        public List<MenuItem> SpecialItems { get; set; } = new List<MenuItem>();

        // ==================== USER ORDERS ====================
        
        /// <summary>
        /// User's recent orders (last 10)
        /// </summary>
        public List<Order> RecentOrders { get; set; } = new List<Order>();

        /// <summary>
        /// User's saved favorite orders
        /// </summary>
        public List<SavedOrder> SavedOrders { get; set; } = new List<SavedOrder>();

        /// <summary>
        /// IDs of saved orders currently applied to cart
        /// </summary>
        public List<int> AppliedSavedOrderIds { get; set; } = new List<int>();

        // ==================== CART SUMMARY ====================
        
        /// <summary>
        /// Current cart items
        /// </summary>
        public List<OrderItem> CartItems { get; set; } = new List<OrderItem>();

        /// <summary>
        /// Total number of items in cart
        /// </summary>
        public int CartItemCount => CartItems.Sum(item => item.Quantity);

        /// <summary>
        /// Cart subtotal (before discounts)
        /// </summary>
        public decimal CartSubtotal { get; set; }

        /// <summary>
        /// Whether user has items in cart
        /// </summary>
        public bool HasItemsInCart => CartItems.Any();

        // ==================== USER INFO ====================
        
        /// <summary>
        /// Whether user is authenticated
        /// </summary>
        public bool IsAuthenticated { get; set; }

        /// <summary>
        /// Whether user is a staff member (eligible for discount)
        /// </summary>
        public bool IsStaff { get; set; }

        /// <summary>
        /// Current user's customer ID
        /// </summary>
        public string? CustomerId { get; set; }

        // ==================== AVAILABLE MODIFIERS ====================
        
        /// <summary>
        /// Available modifiers for drinks
        /// </summary>
        public List<ModifierOption> DrinkModifiers { get; set; } = new List<ModifierOption>();

        /// <summary>
        /// Available modifiers for food
        /// </summary>
        public List<ModifierOption> FoodModifiers { get; set; } = new List<ModifierOption>();

        // ==================== HELPER METHODS ====================
        
        /// <summary>
        /// Checks if a specific menu item is available during current service period
        /// </summary>
        public bool IsItemAvailable(string itemName)
        {
            if (IsDebugMode) return true;

            var item = AllMenuItems.FirstOrDefault(i => i.Name == itemName);
            if (item == null) return false;

            return CurrentServicePeriod switch
            {
                ServicePeriod.Breakfast => item.AvailableDuringBreakfast,
                ServicePeriod.Lunch => item.AvailableDuringLunch,
                _ => false
            };
        }

        /// <summary>
        /// Gets availability message for an item
        /// </summary>
        public string GetAvailabilityMessage(string itemName)
        {
            if (IsItemAvailable(itemName)) return string.Empty;

            var item = AllMenuItems.FirstOrDefault(i => i.Name == itemName);
            if (item == null) return "Item not found";

            if (item.AvailableDuringBreakfast && !item.AvailableDuringLunch)
                return "Available during Breakfast hours";
            if (item.AvailableDuringLunch && !item.AvailableDuringBreakfast)
                return "Available during Lunch hours";

            return "Currently unavailable";
        }
    }

    /// <summary>
    /// Represents a modifier option with its price adjustment
    /// </summary>
    public class ModifierOption
    {
        /// <summary>
        /// Modifier name (e.g., "Extra Shot", "Oat Milk")
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// Display label for UI
        /// </summary>
        public required string Label { get; set; }

        /// <summary>
        /// Price adjustment (can be positive or zero)
        /// </summary>
        public decimal PriceAdjustment { get; set; }

        /// <summary>
        /// Checkbox ID for HTML form
        /// </summary>
        public required string CheckboxId { get; set; }

        /// <summary>
        /// Whether this modifier has an additional cost
        /// </summary>
        public bool HasCost => PriceAdjustment > 0;

        /// <summary>
        /// Formatted price display (e.g., "+$0.75")
        /// </summary>
        public string PriceDisplay => HasCost ? $"+${PriceAdjustment:F2}" : string.Empty;
    }
}
