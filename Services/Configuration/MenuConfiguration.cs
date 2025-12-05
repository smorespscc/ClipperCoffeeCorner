using ClipperCoffeeCorner.Models.Domain;
using System;
using System.Collections.Generic;

namespace ClipperCoffeeCorner.Services.Configuration
{
    /// <summary>
    /// Central configuration for menu items, pricing, and service hours.
    /// This class contains all hardcoded menu data that will eventually move to database.
    /// 
    /// FUTURE DATABASE MIGRATION:
    /// When moving to SQL database, this data will be seeded into:
    /// - MenuItems table
    /// - ModifierOptions table
    /// - ServiceHours configuration table
    /// 
    /// For now, this provides a single source of truth for menu configuration.
    /// </summary>
    public static class MenuConfiguration
    {
        // ==================== SERVICE HOURS ====================
        
        /// <summary>
        /// Breakfast service start time (Pacific Time)
        /// </summary>
        public static readonly TimeSpan BreakfastStart = new TimeSpan(8, 0, 0); // 8:00 AM

        /// <summary>
        /// Breakfast service end time (Pacific Time)
        /// </summary>
        public static readonly TimeSpan BreakfastEnd = new TimeSpan(10, 30, 0); // 10:30 AM

        /// <summary>
        /// Lunch service end time (Pacific Time)
        /// Lunch starts when breakfast ends
        /// </summary>
        public static readonly TimeSpan LunchEnd = new TimeSpan(14, 0, 0); // 2:00 PM

        // ==================== BASE PRICES ====================
        
        /// <summary>
        /// Base prices for all menu items (in USD)
        /// Key: Item name, Value: Price
        /// </summary>
        public static readonly Dictionary<string, decimal> BasePrices = new Dictionary<string, decimal>
        {
            // Hot Coffee Drinks - Lattes
            { "Chai Latte", 4.75m },
            { "Matcha Latte", 4.95m },
            { "Regular Latte", 4.50m },
            
            // Hot Coffee - Americano & Espresso
            { "Americano", 3.75m },
            { "Espresso", 2.75m },
            
            // Hot Coffee - Mocha
            { "Plain Mocha", 4.95m },
            { "White Mocha", 5.25m },
            
            // Hot Coffee - Drip
            { "Drip Coffee", 3.50m },
            
            // Cold Coffee - Lattes
            { "Iced Latte", 4.75m },
            { "Iced Chai Latte", 4.95m },
            { "Iced Matcha Latte", 5.15m },
            
            // Cold - Blended & Other
            { "Blended Coffee", 5.50m },
            { "Juice", 3.95m },
            { "Water", 1.50m },
            
            // Breakfast Food Items
            { "Breakfast Sandwich", 6.50m },
            { "Bagel with Cream Cheese", 4.25m },
            { "Croissant", 3.75m },
            { "Blueberry Muffin", 3.50m },
            { "Pancakes", 7.50m },
            { "French Toast", 7.95m },
            { "Breakfast Burrito", 8.50m },
            { "Avocado Toast", 7.25m },
            
            // Lunch Food Items
            { "Turkey Sandwich", 8.95m },
            { "Chicken Wrap", 9.50m },
            { "Caesar Salad", 8.25m },
            { "Soup of the Day", 6.50m },
            { "Grilled Cheese", 6.95m },
            { "Club Sandwich", 9.95m }
        };

        // ==================== MODIFIER PRICES ====================
        
        /// <summary>
        /// Price adjustments for modifiers (in USD)
        /// Key: Modifier name, Value: Price adjustment
        /// </summary>
        public static readonly Dictionary<string, decimal> ModifierPrices = new Dictionary<string, decimal>
        {
            // Milk alternatives (non-dairy) - with cost
            { "Almond Milk", 0.50m },
            { "Cashew Milk", 0.50m },
            { "Oat Milk", 0.50m },
            { "Soy Milk", 0.50m },
            { "Coconut Milk", 0.50m },
            
            // Dairy options (regular) - with cost
            { "Breve", 0.75m },  // Half and half
            { "Heavy Cream", 0.75m },
            { "Nonfat Milk", 0.00m },
            { "Whole Milk", 0.00m },
            
            // Espresso shots - with cost
            { "Extra Shot", 0.75m },
            { "1 Shot", 0.00m },
            { "2 Shots", 0.75m },
            { "3 Shots", 1.50m },
            { "4 Shots", 2.25m },
            { "5 Shots", 3.00m },
            { "6 Shots", 3.75m },
            
            // Size options - with cost
            { "Small", 0.00m },
            { "Medium", 0.50m },
            { "Large", 1.00m },
            
            // Ice options - free
            { "No Ice", 0.00m },
            { "Light Ice", 0.00m },
            { "Regular Ice", 0.00m },
            { "Extra Ice", 0.00m },
            
            // Temperature - free
            { "Hot", 0.00m },
            { "Iced", 0.00m },
            { "Blended", 0.00m }
        };

        // ==================== MENU ITEM AVAILABILITY ====================
        
        /// <summary>
        /// Items available during breakfast service (8:00 AM - 10:30 AM)
        /// </summary>
        public static readonly HashSet<string> BreakfastItems = new HashSet<string>
        {
            // Hot Coffee Drinks
            "Chai Latte", "Matcha Latte", "Regular Latte", "Americano", "Espresso",
            "Plain Mocha", "White Mocha", "Drip Coffee",
            
            // Cold Drinks
            "Iced Latte", "Iced Chai Latte", "Iced Matcha Latte", 
            "Blended Coffee", "Juice", "Water",
            
            // Breakfast Food
            "Breakfast Sandwich", "Bagel with Cream Cheese", "Croissant", 
            "Blueberry Muffin", "Pancakes", "French Toast", 
            "Breakfast Burrito", "Avocado Toast"
        };

        /// <summary>
        /// Items available during lunch service (10:30 AM - 2:00 PM)
        /// </summary>
        public static readonly HashSet<string> LunchItems = new HashSet<string>
        {
            // Hot Coffee Drinks
            "Chai Latte", "Matcha Latte", "Regular Latte", "Americano", "Espresso",
            "Plain Mocha", "White Mocha", "Drip Coffee",
            
            // Cold Drinks
            "Iced Latte", "Iced Chai Latte", "Iced Matcha Latte", 
            "Blended Coffee", "Juice", "Water",
            
            // Lunch Food
            "Turkey Sandwich", "Chicken Wrap", "Caesar Salad", 
            "Soup of the Day", "Grilled Cheese", "Club Sandwich"
        };

        // ==================== MENU CATEGORIES ====================
        
        /// <summary>
        /// Items featured in "Trending" section
        /// </summary>
        public static readonly HashSet<string> TrendingItems = new HashSet<string>
        {
            "Iced Matcha Latte", "Blended Coffee"
        };

        /// <summary>
        /// Items featured in "Specials" section
        /// </summary>
        public static readonly HashSet<string> SpecialItems = new HashSet<string>
        {
            "Chai Latte", "Drip Coffee"
        };

        // ==================== DRINK MODIFIERS ====================
        
        /// <summary>
        /// Available modifiers for drink items
        /// Grouped by category for UI organization
        /// </summary>
        public static readonly Dictionary<string, List<string>> DrinkModifierGroups = new Dictionary<string, List<string>>
        {
            { "Temperature", new List<string> { "Hot", "Iced", "Blended" } },
            { "Dairy", new List<string> { "Breve", "Heavy Cream", "Nonfat Milk", "Whole Milk" } },
            { "Non-Dairy", new List<string> { "Almond Milk", "Cashew Milk", "Oat Milk", "Soy Milk", "Coconut Milk" } },
            { "Espresso Shots", new List<string> { "1 Shot", "2 Shots", "3 Shots", "4 Shots", "5 Shots", "6 Shots" } },
            { "Size", new List<string> { "Small", "Medium", "Large" } },
            { "Ice", new List<string> { "No Ice", "Light Ice", "Regular Ice", "Extra Ice" } }
        };

        /// <summary>
        /// All drink modifiers (flattened list)
        /// </summary>
        public static readonly List<string> AllDrinkModifiers = new List<string>
        {
            "Hot", "Iced", "Blended",
            "Breve", "Heavy Cream", "Nonfat Milk", "Whole Milk",
            "Almond Milk", "Cashew Milk", "Oat Milk", "Soy Milk", "Coconut Milk",
            "1 Shot", "2 Shots", "3 Shots", "4 Shots", "5 Shots", "6 Shots",
            "Small", "Medium", "Large",
            "No Ice", "Light Ice", "Regular Ice", "Extra Ice"
        };

        // ==================== FOOD MODIFIERS ====================
        
        /// <summary>
        /// Available modifiers for food items
        /// Grouped by category for UI organization
        /// </summary>
        public static readonly Dictionary<string, List<string>> FoodModifierGroups = new Dictionary<string, List<string>>
        {
            // No food items in current menu
        };

        /// <summary>
        /// All food modifiers (flattened list)
        /// </summary>
        public static readonly List<string> AllFoodModifiers = new List<string>
        {
            // No food items in current menu
        };

        // ==================== STAFF DISCOUNT ====================
        
        /// <summary>
        /// Staff discount percentage (10%)
        /// </summary>
        public const decimal StaffDiscountPercentage = 0.10m;

        // ==================== HELPER METHODS ====================
        
        /// <summary>
        /// Gets base price for an item, returns default if not found
        /// </summary>
        public static decimal GetBasePrice(string itemName, decimal defaultPrice = 4.00m)
        {
            return BasePrices.TryGetValue(itemName, out decimal price) ? price : defaultPrice;
        }

        /// <summary>
        /// Gets modifier price adjustment
        /// </summary>
        public static decimal GetModifierPrice(string modifierName)
        {
            return ModifierPrices.TryGetValue(modifierName, out decimal price) ? price : 0.00m;
        }

        /// <summary>
        /// Checks if item is available during breakfast
        /// </summary>
        public static bool IsAvailableDuringBreakfast(string itemName)
        {
            return BreakfastItems.Contains(itemName);
        }

        /// <summary>
        /// Checks if item is available during lunch
        /// </summary>
        public static bool IsAvailableDuringLunch(string itemName)
        {
            return LunchItems.Contains(itemName);
        }

        /// <summary>
        /// Checks if item is trending
        /// </summary>
        public static bool IsTrending(string itemName)
        {
            return TrendingItems.Contains(itemName);
        }

        /// <summary>
        /// Checks if item is a special
        /// </summary>
        public static bool IsSpecial(string itemName)
        {
            return SpecialItems.Contains(itemName);
        }

        /// <summary>
        /// Calculates price with modifiers
        /// </summary>
        public static decimal CalculatePriceWithModifiers(string itemName, List<string> modifiers)
        {
            decimal price = GetBasePrice(itemName);
            
            if (modifiers != null)
            {
                foreach (var modifier in modifiers)
                {
                    price += GetModifierPrice(modifier);
                }
            }
            
            return price;
        }
    }
}
