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
                    IsSpecial = MenuConfiguration.IsSpecial(name)
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
            var drinkKeywords = new[] { "Coffee", "Latte", "Brew", "Tea", "Mocha", "Espresso", "Americano", "Macchiato", "Cappuccino", "Cortado", "Flat White" };

            foreach (var keyword in drinkKeywords)
            {
                if (itemName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return "Drink";
                }
            }

            return "Food";
        }
    }
}
