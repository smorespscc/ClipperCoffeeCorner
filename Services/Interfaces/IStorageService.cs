using ClipperCoffeeCorner.Models;
using ClipperCoffeeCorner.Models.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClipperCoffeeCorner.Services.Interfaces
{
    /// <summary>
    /// Abstraction layer for all data persistence operations.
    /// This interface supports both localStorage (current) and SQL database (future).
    /// 
    /// IMPLEMENTATION NOTES:
    /// - LocalStorageService: Uses client-side localStorage via JavaScript bridge
    /// - DatabaseStorageService: Will use Entity Framework Core with SQL Server
    /// 
    /// FUTURE DATABASE INTEGRATION:
    /// When migrating to SQL, implement this interface with EF Core DbContext.
    /// No changes needed in controllers or services - just swap the DI registration.
    /// </summary>
    public interface IStorageService
    {
        // ==================== CUSTOMER OPERATIONS ====================
        
        /// <summary>
        /// Saves customer data (registration, profile updates)
        /// FUTURE: INSERT/UPDATE to Customers table
        /// </summary>
        Task<bool> SaveCustomerAsync(Customer customer);
        
        /// <summary>
        /// Retrieves customer by email
        /// FUTURE: SELECT FROM Customers WHERE Email = @email
        /// </summary>
        Task<Customer?> GetCustomerByEmailAsync(string email);
        
        /// <summary>
        /// Validates customer credentials
        /// FUTURE: Check against hashed password in database
        /// </summary>
        Task<bool> ValidateCustomerCredentialsAsync(string email, string password);
        
        /// <summary>
        /// Checks if staff code is valid
        /// FUTURE: SELECT FROM StaffCodes WHERE Code = @code AND IsActive = 1
        /// </summary>
        Task<bool> ValidateStaffCodeAsync(string staffCode);

        // ==================== ORDER OPERATIONS ====================
        
        /// <summary>
        /// Saves current cart items
        /// FUTURE: Store in session or temporary Orders table
        /// </summary>
        Task<bool> SaveCartItemsAsync(string sessionId, List<OrderItem> items);
        
        /// <summary>
        /// Retrieves current cart items
        /// FUTURE: SELECT FROM OrderItems WHERE SessionId = @sessionId
        /// </summary>
        Task<List<OrderItem>> GetCartItemsAsync(string sessionId);
        
        /// <summary>
        /// Clears cart items
        /// FUTURE: DELETE FROM OrderItems WHERE SessionId = @sessionId
        /// </summary>
        Task<bool> ClearCartAsync(string sessionId);
        
        /// <summary>
        /// Saves a completed order
        /// FUTURE: INSERT INTO Orders with related OrderItems
        /// </summary>
        Task<int> SaveOrderAsync(Order order);
        
        /// <summary>
        /// Retrieves the last completed order for a customer
        /// FUTURE: SELECT TOP 1 FROM Orders WHERE CustomerId = @id ORDER BY CreatedAt DESC
        /// </summary>
        Task<Order?> GetLastOrderAsync(string customerId);

        // ==================== SAVED ORDERS ====================
        
        /// <summary>
        /// Saves a customer's favorite order configuration
        /// FUTURE: INSERT INTO SavedOrders with related SavedOrderItems
        /// </summary>
        Task<int> SaveFavoriteOrderAsync(SavedOrder savedOrder);
        
        /// <summary>
        /// Retrieves all saved orders for a customer
        /// FUTURE: SELECT FROM SavedOrders WHERE CustomerId = @id
        /// </summary>
        Task<List<SavedOrder>> GetSavedOrdersAsync(string customerId);
        
        /// <summary>
        /// Deletes a saved order
        /// FUTURE: DELETE FROM SavedOrders WHERE Id = @id
        /// </summary>
        Task<bool> DeleteSavedOrderAsync(int savedOrderId);
        
        /// <summary>
        /// Updates an existing saved order
        /// FUTURE: UPDATE SavedOrders SET ... WHERE Id = @id
        /// </summary>
        Task<bool> UpdateSavedOrderAsync(SavedOrder savedOrder);

        // ==================== RECENT ORDERS ====================
        
        /// <summary>
        /// Adds an order to recent orders history
        /// FUTURE: Already stored in Orders table, just query recent
        /// </summary>
        Task<bool> AddToRecentOrdersAsync(string customerId, Order order);
        
        /// <summary>
        /// Retrieves recent orders for a customer (last 10)
        /// FUTURE: SELECT TOP 10 FROM Orders WHERE CustomerId = @id ORDER BY CreatedAt DESC
        /// </summary>
        Task<List<Order>> GetRecentOrdersAsync(string customerId);

        // ==================== PREFERENCES & SETTINGS ====================
        
        /// <summary>
        /// Saves customer notification preferences
        /// FUTURE: UPDATE Customers SET EmailNotifications = @email, TextNotifications = @text
        /// </summary>
        Task<bool> SaveNotificationPreferencesAsync(string customerId, bool emailConsent, bool textConsent);
        
        /// <summary>
        /// Retrieves customer notification preferences
        /// FUTURE: SELECT EmailNotifications, TextNotifications FROM Customers WHERE Id = @id
        /// </summary>
        Task<(bool emailConsent, bool textConsent)> GetNotificationPreferencesAsync(string customerId);
        
        /// <summary>
        /// Saves special requests for current order
        /// FUTURE: Store in session or OrderItems.SpecialRequests
        /// </summary>
        Task<bool> SaveSpecialRequestsAsync(string sessionId, string specialRequests);
        
        /// <summary>
        /// Retrieves special requests for current order
        /// FUTURE: SELECT SpecialRequests FROM Sessions WHERE Id = @sessionId
        /// </summary>
        Task<string?> GetSpecialRequestsAsync(string sessionId);

        // ==================== SESSION MANAGEMENT ====================
        
        /// <summary>
        /// Checks if user is authenticated
        /// FUTURE: Validate session token against database
        /// </summary>
        Task<bool> IsAuthenticatedAsync(string sessionId);
        
        /// <summary>
        /// Checks if user is staff member
        /// FUTURE: SELECT IsStaff FROM Customers WHERE SessionId = @sessionId
        /// </summary>
        Task<bool> IsStaffMemberAsync(string sessionId);
        
        /// <summary>
        /// Gets current customer ID from session
        /// FUTURE: SELECT CustomerId FROM Sessions WHERE Id = @sessionId
        /// </summary>
        Task<string?> GetCurrentCustomerIdAsync(string sessionId);
    }
}
