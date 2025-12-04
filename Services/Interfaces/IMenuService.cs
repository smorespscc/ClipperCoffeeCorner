using ClipperCoffeeCorner.Models.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClipperCoffeeCorner.Services.Interfaces
{
    /// <summary>
    /// Business logic for menu operations.
    /// Handles menu item availability, service periods, and menu configuration.
    /// </summary>
    public interface IMenuService
    {
        // ==================== MENU ITEMS ====================
        
        /// <summary>
        /// Retrieves all menu items
        /// </summary>
        Task<List<MenuItem>> GetAllMenuItemsAsync();
        
        /// <summary>
        /// Retrieves menu items available during current service period
        /// </summary>
        Task<List<MenuItem>> GetAvailableMenuItemsAsync();
        
        /// <summary>
        /// Retrieves menu items for a specific service period
        /// </summary>
        Task<List<MenuItem>> GetMenuItemsByPeriodAsync(ServicePeriod period);
        
        /// <summary>
        /// Gets a specific menu item by name
        /// </summary>
        Task<MenuItem?> GetMenuItemByNameAsync(string itemName);
        
        /// <summary>
        /// Checks if a menu item is available during current service period
        /// </summary>
        Task<bool> IsItemAvailableAsync(string itemName);

        // ==================== SERVICE PERIODS ====================
        
        /// <summary>
        /// Determines current service period based on Pacific Time
        /// Returns: Breakfast, Lunch, or Closed
        /// </summary>
        Task<ServicePeriod> GetCurrentServicePeriodAsync();
        
        /// <summary>
        /// Gets service hours for a specific period
        /// </summary>
        Task<(TimeSpan start, TimeSpan end)> GetServiceHoursAsync(ServicePeriod period);
        
        /// <summary>
        /// Checks if cafe is currently open
        /// </summary>
        Task<bool> IsCurrentlyOpenAsync();
        
        /// <summary>
        /// Gets time until next service period opens
        /// </summary>
        Task<TimeSpan?> GetTimeUntilNextOpenAsync();

        // ==================== MENU CATEGORIES ====================
        
        /// <summary>
        /// Retrieves trending menu items
        /// </summary>
        Task<List<MenuItem>> GetTrendingItemsAsync();
        
        /// <summary>
        /// Retrieves special menu items
        /// </summary>
        Task<List<MenuItem>> GetSpecialItemsAsync();
        
        /// <summary>
        /// Retrieves menu items by type (drink or food)
        /// </summary>
        Task<List<MenuItem>> GetMenuItemsByTypeAsync(MenuItemType type);

        // ==================== PRICING ====================
        
        /// <summary>
        /// Gets base price for a menu item
        /// </summary>
        Task<decimal> GetBasePriceAsync(string itemName);
        
        /// <summary>
        /// Calculates price with modifiers applied
        /// </summary>
        Task<decimal> CalculateItemPriceAsync(string itemName, List<string> modifiers);
        
        /// <summary>
        /// Gets all available modifiers for an item type
        /// </summary>
        Task<List<string>> GetAvailableModifiersAsync(MenuItemType type);
        
        /// <summary>
        /// Gets modifier price adjustment
        /// </summary>
        Task<decimal> GetModifierPriceAsync(string modifierName);
    }

    /// <summary>
    /// Service period enumeration
    /// </summary>
    public enum ServicePeriod
    {
        Closed,
        Breakfast,
        Lunch
    }

    /// <summary>
    /// Menu item type enumeration
    /// </summary>
    public enum MenuItemType
    {
        Drink,
        Food
    }
}
