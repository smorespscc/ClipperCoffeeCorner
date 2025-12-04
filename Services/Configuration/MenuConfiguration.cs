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
            // Drinks
            { "Cappuccino", 4.50m },
            { "Vanilla Latte", 4.95m },
            { "House Brew", 3.50m },
            { "Chai Tea Latte", 4.75m },
            { "Iced Mocha", 5.25m },
            { "Double Espresso", 3.25m },
            { "Espresso", 2.75m },
            { "Americano", 3.75m },
            { "Macchiato", 3.95m },
            { "Mocha", 4.95m },
            { "Flat White", 4.25m },
            { "Cortado", 3.50m },
            
            // Food
            { "Breakfast Sandwich", 5.50m },
            { "Bagel & Cream Cheese", 3.25m },
            { "Pancake Stack", 6.50m },
            { "Cheese Omelette", 6.00m },
            { "Butter Croissant", 2.75m },
            { "Blueberry Muffin", 2.95m },
            { "French Toast", 6.75m },
            { "Avocado Toast", 7.25m },
            { "Breakfast Burrito", 7.50m },
            { "Belgian Waffle", 6.95m },
            { "Eggs Benedict", 8.50m },
            { "Breakfast Bowl", 7.95m }
        };

        // ==================== MODIFIER PRICES ====================
        
        /// <summary>
        /// Price adjustments for modifiers (in USD)
        /// Key: Modifier name, Value: Price adjustment
        /// </summary>
        public static readonly Dictionary<string, decimal> ModifierPrices = new Dictionary<string, decimal>
        {
            // Drink modifiers with cost
            { "Extra Shot", 0.75m },
            { "Oat Milk", 0.50m },
            { "Oat", 0.50m },
            { "Almond", 0.50m },
            { "Soy", 0.50m },
            { "Whip Cream", 0.25m },
            { "Whip", 0.25m },
            
            // Free modifiers (no cost)
            { "Hot", 0.00m },
            { "Iced", 0.00m },
            { "Dairy", 0.00m },
            { "Vanilla", 0.00m },
            { "Mint", 0.00m },
            { "Cinnamon", 0.00m },
            { "Hazelnut", 0.00m },
            { "Caramel", 0.00m },
            { "Decaf", 0.00m },
            { "Sugar Free", 0.00m },
            { "Caffeine Free", 0.00m },
            
            // Food modifiers (all free)
            { "Well Done", 0.00m },
            { "Medium", 0.00m },
            { "Light", 0.00m },
            { "Hash Browns", 0.00m },
            { "Fresh Fruit", 0.00m },
            { "Toast", 0.00m },
            { "Extra Cheese", 0.00m },
            { "Bacon", 0.00m },
            { "Avocado", 0.00m },
            { "Sausage", 0.00m },
            { "Gluten Free", 0.00m },
            { "Vegetarian", 0.00m },
            { "Vegan", 0.00m },
            { "No Dairy", 0.00m },
            { "No Onions", 0.00m },
            { "No Tomatoes", 0.00m },
            { "Extra Crispy", 0.00m }
        };

        // ==================== MENU ITEM AVAILABILITY ====================
        
        /// <summary>
        /// Items available during breakfast service (8:00 AM - 10:30 AM)
        /// </summary>
        public static readonly HashSet<string> BreakfastItems = new HashSet<string>
        {
            // Drinks
            "Cappuccino", "Vanilla Latte", "House Brew", "Chai Tea Latte", 
            "Espresso", "Americano", "Macchiato", "Flat White", "Cortado", 
            "Double Espresso",
            
            // Food
            "Butter Croissant", "Blueberry Muffin", "Pancake Stack", 
            "Cheese Omelette", "French Toast", "Breakfast Sandwich", 
            "Bagel & Cream Cheese", "Belgian Waffle", "Eggs Benedict", 
            "Breakfast Bowl", "Breakfast Burrito"
        };

        /// <summary>
        /// Items available during lunch service (10:30 AM - 2:00 PM)
        /// </summary>
        public static readonly HashSet<string> LunchItems = new HashSet<string>
        {
            // Drinks
            "Iced Mocha", "Chai Tea Latte", "House Brew", "Americano", "Espresso",
            
            // Food
            "Avocado Toast", "Breakfast Sandwich", "Bagel & Cream Cheese"
        };

        // ==================== MENU CATEGORIES ====================
        
        /// <summary>
        /// Items featured in "Trending" section
        /// </summary>
        public static readonly HashSet<string> TrendingItems = new HashSet<string>
        {
            "Iced Mocha", "Double Espresso"
        };

        /// <summary>
        /// Items featured in "Specials" section
        /// </summary>
        public static readonly HashSet<string> SpecialItems = new HashSet<string>
        {
            "Breakfast Sandwich", "Bagel & Cream Cheese"
        };

        // ==================== DRINK MODIFIERS ====================
        
        /// <summary>
        /// Available modifiers for drink items
        /// Grouped by category for UI organization
        /// </summary>
        public static readonly Dictionary<string, List<string>> DrinkModifierGroups = new Dictionary<string, List<string>>
        {
            { "Temperature", new List<string> { "Hot", "Iced" } },
            { "Milk", new List<string> { "Dairy", "Oat", "Almond", "Soy" } },
            { "Flavor", new List<string> { "Vanilla", "Mint", "Cinnamon", "Hazelnut", "Caramel" } },
            { "Extras", new List<string> { "Whip Cream", "Extra Shot", "Decaf", "Sugar Free", "Caffeine Free" } }
        };

        /// <summary>
        /// All drink modifiers (flattened list)
        /// </summary>
        public static readonly List<string> AllDrinkModifiers = new List<string>
        {
            "Hot", "Iced", "Dairy", "Oat", "Almond", "Soy", "Vanilla", "Mint", 
            "Cinnamon", "Hazelnut", "Caramel", "Whip Cream", "Extra Shot", 
            "Decaf", "Sugar Free", "Caffeine Free"
        };

        // ==================== FOOD MODIFIERS ====================
        
        /// <summary>
        /// Available modifiers for food items
        /// Grouped by category for UI organization
        /// </summary>
        public static readonly Dictionary<string, List<string>> FoodModifierGroups = new Dictionary<string, List<string>>
        {
            { "Cooking", new List<string> { "Well Done", "Medium", "Light", "Extra Crispy" } },
            { "Sides", new List<string> { "Hash Browns", "Fresh Fruit", "Toast" } },
            { "Add-ons", new List<string> { "Extra Cheese", "Bacon", "Avocado", "Sausage" } },
            { "Dietary", new List<string> { "Gluten Free", "Vegetarian", "Vegan", "No Dairy", "No Onions", "No Tomatoes" } }
        };

        /// <summary>
        /// All food modifiers (flattened list)
        /// </summary>
        public static readonly List<string> AllFoodModifiers = new List<string>
        {
            "Well Done", "Medium", "Light", "Hash Browns", "Fresh Fruit", "Toast",
            "Extra Cheese", "Bacon", "Avocado", "Sausage", "Gluten Free", 
            "Vegetarian", "Vegan", "No Dairy", "No Onions", "No Tomatoes", "Extra Crispy"
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
