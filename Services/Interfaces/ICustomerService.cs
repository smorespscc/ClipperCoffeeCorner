using ClipperCoffeeCorner.Models;
using ClipperCoffeeCorner.Models.Domain;
using System.Threading.Tasks;

namespace ClipperCoffeeCorner.Services.Interfaces
{
    /// <summary>
    /// Business logic for customer authentication and management.
    /// Handles login, registration, and staff validation.
    /// </summary>
    public interface ICustomerService
    {
        // ==================== AUTHENTICATION ====================
        
        /// <summary>
        /// Authenticates a customer with email and password
        /// </summary>
        Task<(bool success, string? customerId, string? errorMessage)> AuthenticateAsync(
            string email, 
            string password);
        
        /// <summary>
        /// Validates staff code for discount eligibility
        /// </summary>
        Task<bool> ValidateStaffCodeAsync(string staffCode);
        
        /// <summary>
        /// Logs out current customer
        /// </summary>
        Task<bool> LogoutAsync(string sessionId);

        // ==================== REGISTRATION ====================
        
        /// <summary>
        /// Registers a new customer
        /// </summary>
        Task<(bool success, string? customerId, string? errorMessage)> RegisterCustomerAsync(
            string username,
            string email,
            string phone,
            string password);
        
        /// <summary>
        /// Checks if email is already registered
        /// </summary>
        Task<bool> IsEmailRegisteredAsync(string email);

        // ==================== PROFILE MANAGEMENT ====================
        
        /// <summary>
        /// Gets customer profile
        /// </summary>
        Task<Customer?> GetCustomerProfileAsync(string customerId);
        
        /// <summary>
        /// Updates customer profile
        /// </summary>
        Task<bool> UpdateCustomerProfileAsync(Customer customer);
        
        /// <summary>
        /// Saves notification preferences
        /// </summary>
        Task<bool> SaveNotificationPreferencesAsync(
            string customerId, 
            bool emailConsent, 
            bool textConsent);
        
        /// <summary>
        /// Gets notification preferences
        /// </summary>
        Task<(bool emailConsent, bool textConsent)> GetNotificationPreferencesAsync(string customerId);

        // ==================== SESSION MANAGEMENT ====================
        
        /// <summary>
        /// Gets current customer ID from session
        /// </summary>
        Task<string?> GetCurrentCustomerIdAsync(string sessionId);
        
        /// <summary>
        /// Sets current customer in session
        /// </summary>
        Task<bool> SetCurrentCustomerAsync(string sessionId, string customerId, bool isStaff);
        
        /// <summary>
        /// Checks if current session is staff
        /// </summary>
        Task<bool> IsStaffSessionAsync(string sessionId);
    }
}
