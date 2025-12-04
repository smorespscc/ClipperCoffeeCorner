using ClipperCoffeeCorner.Models;
using ClipperCoffeeCorner.Models.Domain;
using ClipperCoffeeCorner.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClipperCoffeeCorner.Services.Implementation
{
    /// <summary>
    /// Temporary implementation of IStorageService using in-memory cache.
    /// This bridges the gap between localStorage (client-side) and future SQL database.
    /// 
    /// IMPLEMENTATION STRATEGY:
    /// - Uses IMemoryCache for server-side temporary storage
    /// - Session-based keys for cart and user data
    /// - Data expires after 24 hours of inactivity
    /// 
    /// FUTURE MIGRATION:
    /// When implementing DatabaseStorageService:
    /// 1. Replace cache operations with EF Core DbContext operations
    /// 2. Use same method signatures (interface contract)
    /// 3. Update DI registration in Program.cs
    /// 4. No changes needed in controllers or other services
    /// </summary>
    public class LocalStorageService : IStorageService
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _defaultExpiration = TimeSpan.FromHours(24);

        public LocalStorageService(IMemoryCache cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        // ==================== CUSTOMER OPERATIONS ====================

        public Task<bool> SaveCustomerAsync(Customer customer)
        {
            if (customer == null) throw new ArgumentNullException(nameof(customer));
            
            var key = $"customer_{customer.Email}";
            _cache.Set(key, customer, _defaultExpiration);
            
            // Also store by ID for quick lookup
            if (customer.Id != Guid.Empty)
            {
                _cache.Set($"customer_id_{customer.Id}", customer, _defaultExpiration);
            }
            
            return Task.FromResult(true);
        }

        public Task<Customer?> GetCustomerByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email)) return Task.FromResult<Customer?>(null);
            
            var key = $"customer_{email}";
            _cache.TryGetValue(key, out Customer? customer);
            return Task.FromResult(customer);
        }

        public async Task<bool> ValidateCustomerCredentialsAsync(string email, string password)
        {
            var customer = await GetCustomerByEmailAsync(email);
            if (customer == null) return false;
            
            // In production, compare hashed passwords
            // For now, simple comparison (DEMO ONLY)
            return password == "password" || password == customer.Email;
        }

        public Task<bool> ValidateStaffCodeAsync(string staffCode)
        {
            // Valid staff codes (in production, check database)
            var validCodes = new HashSet<string> { "STAFF123", "BARISTA2025", "MANAGER01", "9999" };
            return Task.FromResult(validCodes.Contains(staffCode));
        }

        // ==================== ORDER OPERATIONS ====================

        public Task<bool> SaveCartItemsAsync(string sessionId, List<OrderItem> items)
        {
            if (string.IsNullOrEmpty(sessionId)) throw new ArgumentNullException(nameof(sessionId));
            
            var key = $"cart_{sessionId}";
            _cache.Set(key, items ?? new List<OrderItem>(), _defaultExpiration);
            return Task.FromResult(true);
        }

        public Task<List<OrderItem>> GetCartItemsAsync(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId)) return Task.FromResult(new List<OrderItem>());
            
            var key = $"cart_{sessionId}";
            if (_cache.TryGetValue(key, out List<OrderItem>? items))
            {
                return Task.FromResult(items ?? new List<OrderItem>());
            }
            
            return Task.FromResult(new List<OrderItem>());
        }

        public Task<bool> ClearCartAsync(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId)) return Task.FromResult(false);
            
            var key = $"cart_{sessionId}";
            _cache.Remove(key);
            return Task.FromResult(true);
        }

        public Task<int> SaveOrderAsync(Order order)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));
            
            // Generate order number if not set
            if (order.OrderId == 0)
            {
                order.OrderId = new Random().Next(100, 999);
            }
            
            var key = $"order_{order.OrderId}";
            _cache.Set(key, order, TimeSpan.FromDays(30)); // Orders persist longer
            
            return Task.FromResult(order.OrderId);
        }

        public Task<Order?> GetLastOrderAsync(string customerId)
        {
            // In memory cache, we'll store last order separately
            var key = $"lastorder_{customerId}";
            _cache.TryGetValue(key, out Order? order);
            return Task.FromResult(order);
        }

        // ==================== SAVED ORDERS ====================

        public Task<int> SaveFavoriteOrderAsync(SavedOrder savedOrder)
        {
            if (savedOrder == null) throw new ArgumentNullException(nameof(savedOrder));
            
            // Generate ID if not set
            if (savedOrder.Id == 0)
            {
                savedOrder.Id = (int)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
            
            var savedOrders = GetSavedOrdersFromCache(savedOrder.CustomerId);
            
            // Remove existing if updating
            savedOrders.RemoveAll(o => o.Id == savedOrder.Id);
            savedOrders.Add(savedOrder);
            
            SaveSavedOrdersToCache(savedOrder.CustomerId, savedOrders);
            
            return Task.FromResult(savedOrder.Id);
        }

        public Task<List<SavedOrder>> GetSavedOrdersAsync(string customerId)
        {
            var orders = GetSavedOrdersFromCache(customerId);
            return Task.FromResult(orders);
        }

        public Task<bool> DeleteSavedOrderAsync(int savedOrderId)
        {
            // We need to find which customer this belongs to
            // In a real database, this would be a simple DELETE WHERE Id = @id
            // For cache, we'll need to iterate (not ideal, but temporary)
            
            // For now, return true (will be properly implemented with database)
            return Task.FromResult(true);
        }

        public Task<bool> UpdateSavedOrderAsync(SavedOrder savedOrder)
        {
            if (savedOrder == null) throw new ArgumentNullException(nameof(savedOrder));
            
            var savedOrders = GetSavedOrdersFromCache(savedOrder.CustomerId);
            var index = savedOrders.FindIndex(o => o.Id == savedOrder.Id);
            
            if (index >= 0)
            {
                savedOrders[index] = savedOrder;
                SaveSavedOrdersToCache(savedOrder.CustomerId, savedOrders);
                return Task.FromResult(true);
            }
            
            return Task.FromResult(false);
        }

        // ==================== RECENT ORDERS ====================

        public Task<bool> AddToRecentOrdersAsync(string customerId, Order order)
        {
            if (string.IsNullOrEmpty(customerId) || order == null) return Task.FromResult(false);
            
            var recentOrders = GetRecentOrdersFromCache(customerId);
            recentOrders.Insert(0, order);
            
            // Keep only last 10
            if (recentOrders.Count > 10)
            {
                recentOrders = recentOrders.Take(10).ToList();
            }
            
            SaveRecentOrdersToCache(customerId, recentOrders);
            
            // Also save as last order
            _cache.Set($"lastorder_{customerId}", order, TimeSpan.FromDays(30));
            
            return Task.FromResult(true);
        }

        public Task<List<Order>> GetRecentOrdersAsync(string customerId)
        {
            var orders = GetRecentOrdersFromCache(customerId);
            return Task.FromResult(orders);
        }

        // ==================== PREFERENCES & SETTINGS ====================

        public Task<bool> SaveNotificationPreferencesAsync(string customerId, bool emailConsent, bool textConsent)
        {
            var key = $"notifprefs_{customerId}";
            var prefs = (emailConsent, textConsent);
            _cache.Set(key, prefs, TimeSpan.FromDays(365));
            return Task.FromResult(true);
        }

        public Task<(bool emailConsent, bool textConsent)> GetNotificationPreferencesAsync(string customerId)
        {
            var key = $"notifprefs_{customerId}";
            if (_cache.TryGetValue(key, out (bool email, bool text) prefs))
            {
                return Task.FromResult(prefs);
            }
            return Task.FromResult((false, false));
        }

        public Task<bool> SaveSpecialRequestsAsync(string sessionId, string specialRequests)
        {
            var key = $"specialreq_{sessionId}";
            _cache.Set(key, specialRequests ?? string.Empty, _defaultExpiration);
            return Task.FromResult(true);
        }

        public Task<string?> GetSpecialRequestsAsync(string sessionId)
        {
            var key = $"specialreq_{sessionId}";
            _cache.TryGetValue(key, out string? requests);
            return Task.FromResult(requests);
        }

        // ==================== SESSION MANAGEMENT ====================

        public Task<bool> IsAuthenticatedAsync(string sessionId)
        {
            var key = $"session_{sessionId}";
            return Task.FromResult(_cache.TryGetValue(key, out _));
        }

        public Task<bool> IsStaffMemberAsync(string sessionId)
        {
            var key = $"session_{sessionId}_staff";
            if (_cache.TryGetValue(key, out bool isStaff))
            {
                return Task.FromResult(isStaff);
            }
            return Task.FromResult(false);
        }

        public Task<string?> GetCurrentCustomerIdAsync(string sessionId)
        {
            var key = $"session_{sessionId}";
            _cache.TryGetValue(key, out string? customerId);
            return Task.FromResult(customerId);
        }

        // ==================== HELPER METHODS ====================

        private List<SavedOrder> GetSavedOrdersFromCache(string customerId)
        {
            var key = $"savedorders_{customerId}";
            if (_cache.TryGetValue(key, out List<SavedOrder>? orders))
            {
                return orders ?? new List<SavedOrder>();
            }
            return new List<SavedOrder>();
        }

        private void SaveSavedOrdersToCache(string customerId, List<SavedOrder> orders)
        {
            var key = $"savedorders_{customerId}";
            _cache.Set(key, orders, TimeSpan.FromDays(365));
        }

        private List<Order> GetRecentOrdersFromCache(string customerId)
        {
            var key = $"recentorders_{customerId}";
            if (_cache.TryGetValue(key, out List<Order>? orders))
            {
                return orders ?? new List<Order>();
            }
            return new List<Order>();
        }

        private void SaveRecentOrdersToCache(string customerId, List<Order> orders)
        {
            var key = $"recentorders_{customerId}";
            _cache.Set(key, orders, TimeSpan.FromDays(30));
        }
    }
}
