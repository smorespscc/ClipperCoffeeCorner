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
    /// Implementation of menu operations business logic.
    /// Handles menu items, service periods, and availability.
    /// </summary>
    public class MenuService : IMenuService
    {
        // ==================== MENU ITEMS ====================

        public Task<List<MenuItem>> GetAllMenuItemsAsync()
        {
            var items = new List<MenuItem>();
            int id = 1;

            foreach (var (name, price) in MenuConfiguration.BasePrices)
            {
                var item = new MenuItem
                {
                    Id = id++,
                    Name = name,
                    Type = DetermineItemType(name),
                    BasePrice = price,
                    IsAvailable = true,
                    AvailableDuringBreakfast = MenuConfiguration.IsAvailableDuringBreakfast(name),
                    AvailableDuringLunch = MenuConfiguration.IsAvailableDuringLunch(name),
                    IsTrending = MenuConfiguration.IsTrending(name),
                    IsSpecial = MenuConfiguration.IsSpecial(name),
                    ImageUrl = GetImageUrlForItem(name)
                };
                items.Add(item);
            }

            return Task.FromResult(items);
        }

        public async Task<List<MenuItem>> GetAvailableMenuItemsAsync()
        {
            var period = await GetCurrentServicePeriodAsync();
            return await GetMenuItemsByPeriodAsync(period);
        }

        public async Task<List<MenuItem>> GetMenuItemsByPeriodAsync(ServicePeriod period)
        {
            var allItems = await GetAllMenuItemsAsync();

            return period switch
            {
                ServicePeriod.Breakfast => allItems.Where(i => i.AvailableDuringBreakfast).ToList(),
                ServicePeriod.Lunch => allItems.Where(i => i.AvailableDuringLunch).ToList(),
                _ => new List<MenuItem>()
            };
        }

        public async Task<MenuItem?> GetMenuItemByNameAsync(string itemName)
        {
            var items = await GetAllMenuItemsAsync();
            return items.FirstOrDefault(i => i.Name.Equals(itemName, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<bool> IsItemAvailableAsync(string itemName)
        {
            var period = await GetCurrentServicePeriodAsync();
            if (period == ServicePeriod.Closed) return false;

            return period switch
            {
                ServicePeriod.Breakfast => MenuConfiguration.IsAvailableDuringBreakfast(itemName),
                ServicePeriod.Lunch => MenuConfiguration.IsAvailableDuringLunch(itemName),
                _ => false
            };
        }

        // ==================== SERVICE PERIODS ====================

        public Task<ServicePeriod> GetCurrentServicePeriodAsync()
        {
            // Get Pacific Time (cafe is in California)
            var pacificZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            var pacificTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pacificZone);
            var currentTime = pacificTime.TimeOfDay;

            if (currentTime >= MenuConfiguration.BreakfastStart && currentTime < MenuConfiguration.BreakfastEnd)
            {
                return Task.FromResult(ServicePeriod.Breakfast);
            }

            if (currentTime >= MenuConfiguration.BreakfastEnd && currentTime < MenuConfiguration.LunchEnd)
            {
                return Task.FromResult(ServicePeriod.Lunch);
            }

            return Task.FromResult(ServicePeriod.Closed);
        }

        public Task<(TimeSpan start, TimeSpan end)> GetServiceHoursAsync(ServicePeriod period)
        {
            return period switch
            {
                ServicePeriod.Breakfast => Task.FromResult((MenuConfiguration.BreakfastStart, MenuConfiguration.BreakfastEnd)),
                ServicePeriod.Lunch => Task.FromResult((MenuConfiguration.BreakfastEnd, MenuConfiguration.LunchEnd)),
                _ => Task.FromResult((TimeSpan.Zero, TimeSpan.Zero))
            };
        }

        public async Task<bool> IsCurrentlyOpenAsync()
        {
            var period = await GetCurrentServicePeriodAsync();
            return period != ServicePeriod.Closed;
        }

        public async Task<TimeSpan?> GetTimeUntilNextOpenAsync()
        {
            var pacificZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            var pacificTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pacificZone);
            var currentTime = pacificTime.TimeOfDay;

            // If before breakfast, return time until breakfast
            if (currentTime < MenuConfiguration.BreakfastStart)
            {
                return MenuConfiguration.BreakfastStart - currentTime;
            }

            // If after lunch, return time until next day's breakfast
            if (currentTime >= MenuConfiguration.LunchEnd)
            {
                var timeUntilMidnight = TimeSpan.FromDays(1) - currentTime;
                return timeUntilMidnight + MenuConfiguration.BreakfastStart;
            }

            // Currently open
            return null;
        }

        // ==================== MENU CATEGORIES ====================

        public async Task<List<MenuItem>> GetTrendingItemsAsync()
        {
            var items = await GetAllMenuItemsAsync();
            return items.Where(i => i.IsTrending).ToList();
        }

        public async Task<List<MenuItem>> GetSpecialItemsAsync()
        {
            var items = await GetAllMenuItemsAsync();
            return items.Where(i => i.IsSpecial).ToList();
        }

        public async Task<List<MenuItem>> GetMenuItemsByTypeAsync(MenuItemType type)
        {
            var items = await GetAllMenuItemsAsync();
            var typeString = type == MenuItemType.Drink ? "Drink" : "Food";
            return items.Where(i => i.Type == typeString).ToList();
        }

        // ==================== PRICING ====================

        public Task<decimal> GetBasePriceAsync(string itemName)
        {
            var price = MenuConfiguration.GetBasePrice(itemName);
            return Task.FromResult(price);
        }

        public Task<decimal> CalculateItemPriceAsync(string itemName, List<string> modifiers)
        {
            var price = MenuConfiguration.CalculatePriceWithModifiers(itemName, modifiers);
            return Task.FromResult(price);
        }

        public Task<List<string>> GetAvailableModifiersAsync(MenuItemType type)
        {
            var modifiers = type == MenuItemType.Drink
                ? MenuConfiguration.AllDrinkModifiers
                : MenuConfiguration.AllFoodModifiers;

            return Task.FromResult(new List<string>(modifiers));
        }

        public Task<decimal> GetModifierPriceAsync(string modifierName)
        {
            var price = MenuConfiguration.GetModifierPrice(modifierName);
            return Task.FromResult(price);
        }

        // ==================== HELPER METHODS ====================

        private string DetermineItemType(string itemName)
        {
            // Simple heuristic: if it's in drink-related categories, it's a drink
            var drinkKeywords = new[] { "Coffee", "Latte", "Brew", "Tea", "Mocha", "Espresso", "Americano", "Macchiato", "Cappuccino", "Cortado", "Flat White", "Chai", "Matcha", "Juice", "Water", "Blended", "Drip", "Iced" };

            foreach (var keyword in drinkKeywords)
            {
                if (itemName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return "Drink";
                }
            }

            return "Food";
        }

        private string GetImageUrlForItem(string itemName)
        {
            // Map each menu item to high-quality, properly cropped images
            return itemName switch
            {
                // Hot Coffee Drinks
                "Chai Latte" => "https://images.unsplash.com/photo-1578374173705-c08e8c50f71c?q=80&w=1200&auto=format&fit=crop",
                "Matcha Latte" => "https://images.unsplash.com/photo-1536013564743-8e1b7a8105f4?q=80&w=1200&auto=format&fit=crop",
                "Regular Latte" => "https://images.unsplash.com/photo-1461023058943-07fcbe16d735?q=80&w=1200&auto=format&fit=crop",
                "Americano" => "https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd?q=80&w=1200&auto=format&fit=crop",
                "Espresso" => "https://images.unsplash.com/photo-1510591509098-f4fdc6d0ff04?q=80&w=1200&auto=format&fit=crop",
                "Plain Mocha" => "https://images.unsplash.com/photo-1607260550778-aa9d29444ce1?q=80&w=1200&auto=format&fit=crop",
                "White Mocha" => "https://images.unsplash.com/photo-1572442388796-11668a67e53d?q=80&w=1200&auto=format&fit=crop",
                "Drip Coffee" => "https://images.unsplash.com/photo-1497935586351-b67a49e012bf?q=80&w=1200&auto=format&fit=crop",
                
                // Cold Coffee Drinks
                "Iced Latte" => "https://images.unsplash.com/photo-1517487881594-2787fef5ebf7?q=80&w=1200&auto=format&fit=crop",
                "Iced Chai Latte" => "https://images.unsplash.com/photo-1571934811356-5cc061b6821f?q=80&w=1200&auto=format&fit=crop",
                "Iced Matcha Latte" => "https://images.unsplash.com/photo-1564890369478-c89ca6d9cde9?q=80&w=1200&auto=format&fit=crop",
                "Blended Coffee" => "https://images.unsplash.com/photo-1572490122747-3968b75cc699?q=80&w=1200&auto=format&fit=crop",
                
                // Other Drinks
                "Juice" => "https://images.unsplash.com/photo-1600271886742-f049cd451bba?q=80&w=1200&auto=format&fit=crop",
                "Water" => "https://images.unsplash.com/photo-1548839140-29a749e1cf4d?q=80&w=1200&auto=format&fit=crop",
                
                // Breakfast Food
                "Breakfast Sandwich" => "https://images.unsplash.com/photo-1481070555726-e2fe8357725c?q=80&w=1200&auto=format&fit=crop",
                "Bagel with Cream Cheese" => "https://images.unsplash.com/photo-1551106652-a5bcf4b29ab6?q=80&w=1200&auto=format&fit=crop",
                "Croissant" => "https://images.unsplash.com/photo-1555507036-ab1f4038808a?q=80&w=1200&auto=format&fit=crop",
                "Blueberry Muffin" => "https://images.unsplash.com/photo-1607958996333-41aef7caefaa?q=80&w=1200&auto=format&fit=crop",
                "Pancakes" => "https://images.unsplash.com/photo-1528207776546-365bb710ee93?q=80&w=1200&auto=format&fit=crop",
                "French Toast" => "https://images.unsplash.com/photo-1484723091739-30a097e8f929?q=80&w=1200&auto=format&fit=crop",
                "Breakfast Burrito" => "https://images.unsplash.com/photo-1626700051175-6818013e1d4f?q=80&w=1200&auto=format&fit=crop",
                "Avocado Toast" => "https://images.unsplash.com/photo-1541519227354-08fa5d50c44d?q=80&w=1200&auto=format&fit=crop",
                
                // Lunch Food
                "Turkey Sandwich" => "https://images.unsplash.com/photo-1528735602780-2552fd46c7af?q=80&w=1200&auto=format&fit=crop",
                "Chicken Wrap" => "https://images.unsplash.com/photo-1626700051175-6818013e1d4f?q=80&w=1200&auto=format&fit=crop",
                "Caesar Salad" => "https://images.unsplash.com/photo-1546793665-c74683f339c1?q=80&w=1200&auto=format&fit=crop",
                "Soup of the Day" => "https://images.unsplash.com/photo-1547592166-23ac45744acd?q=80&w=1200&auto=format&fit=crop",
                "Grilled Cheese" => "https://images.unsplash.com/photo-1528736235302-52922df5c122?q=80&w=1200&auto=format&fit=crop",
                "Club Sandwich" => "https://images.unsplash.com/photo-1567234669003-dce7a7a88821?q=80&w=1200&auto=format&fit=crop",
                
                _ => "https://images.unsplash.com/photo-1495474472287-4d71bcdd2085?q=80&w=1200&auto=format&fit=crop" // Default coffee image
            };
        }
    }
}
