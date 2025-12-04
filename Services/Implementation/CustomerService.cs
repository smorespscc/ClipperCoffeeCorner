using ClipperCoffeeCorner.Models;
using ClipperCoffeeCorner.Models.Domain;
using ClipperCoffeeCorner.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace ClipperCoffeeCorner.Services.Implementation
{
    /// <summary>
    /// Implementation of customer authentication and management.
    /// Handles login, registration, and staff validation.
    /// </summary>
    public class CustomerService : ICustomerService
    {
        private readonly IStorageService _storageService;
        private readonly IMemoryCache _cache;

        public CustomerService(IStorageService storageService, IMemoryCache cache)
        {
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        // ==================== AUTHENTICATION ====================

        public async Task<(bool success, string? customerId, string? errorMessage)> AuthenticateAsync(
            string email,
            string password)
        {
            if (string.IsNullOrEmpty(email))
                return (false, null, "Email is required");

            if (string.IsNullOrEmpty(password))
                return (false, null, "Password is required");

            // Validate credentials
            var isValid = await _storageService.ValidateCustomerCredentialsAsync(email, password);
            if (!isValid)
                return (false, null, "Invalid email or password");

            // Get customer
            var customer = await _storageService.GetCustomerByEmailAsync(email);
            if (customer == null)
            {
                // For demo: create customer if doesn't exist
                customer = new Customer
                {
                    Id = Guid.NewGuid(),
                    Email = email,
                    SquareCustomerId = Guid.NewGuid().ToString(),
                    PhoneNumber = "000-000-0000"
                };
                await _storageService.SaveCustomerAsync(customer);
            }

            return (true, customer.Id.ToString(), null);
        }

        public async Task<bool> ValidateStaffCodeAsync(string staffCode)
        {
            if (string.IsNullOrEmpty(staffCode)) return false;
            return await _storageService.ValidateStaffCodeAsync(staffCode);
        }

        public async Task<bool> LogoutAsync(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId)) return false;

            // Clear session data
            _cache.Remove($"session_{sessionId}");
            _cache.Remove($"session_{sessionId}_staff");

            return await Task.FromResult(true);
        }

        // ==================== REGISTRATION ====================

        public async Task<(bool success, string? customerId, string? errorMessage)> RegisterCustomerAsync(
            string username,
            string email,
            string phone,
            string password)
        {
            // Validate input
            if (string.IsNullOrEmpty(username))
                return (false, null, "Username is required");

            if (string.IsNullOrEmpty(email))
                return (false, null, "Email is required");

            if (string.IsNullOrEmpty(phone))
                return (false, null, "Phone number is required");

            if (string.IsNullOrEmpty(password))
                return (false, null, "Password is required");

            // Check if email already exists
            var existingCustomer = await _storageService.GetCustomerByEmailAsync(email);
            if (existingCustomer != null)
                return (false, null, "Email is already registered");

            // Create new customer
            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Email = email,
                SquareCustomerId = Guid.NewGuid().ToString(),
                PhoneNumber = phone
            };

            // Save customer
            await _storageService.SaveCustomerAsync(customer);

            return (true, customer.Id.ToString(), null);
        }

        public async Task<bool> IsEmailRegisteredAsync(string email)
        {
            if (string.IsNullOrEmpty(email)) return false;

            var customer = await _storageService.GetCustomerByEmailAsync(email);
            return customer != null;
        }

        // ==================== PROFILE MANAGEMENT ====================

        public async Task<Customer?> GetCustomerProfileAsync(string customerId)
        {
            if (string.IsNullOrEmpty(customerId)) return null;

            // In production, query by customer ID
            // For now, return null (not fully implemented in cache)
            return await Task.FromResult<Customer?>(null);
        }

        public async Task<bool> UpdateCustomerProfileAsync(Customer customer)
        {
            if (customer == null) throw new ArgumentNullException(nameof(customer));

            return await _storageService.SaveCustomerAsync(customer);
        }

        public async Task<bool> SaveNotificationPreferencesAsync(
            string customerId,
            bool emailConsent,
            bool textConsent)
        {
            if (string.IsNullOrEmpty(customerId)) return false;

            return await _storageService.SaveNotificationPreferencesAsync(customerId, emailConsent, textConsent);
        }

        public async Task<(bool emailConsent, bool textConsent)> GetNotificationPreferencesAsync(string customerId)
        {
            if (string.IsNullOrEmpty(customerId)) return (false, false);

            return await _storageService.GetNotificationPreferencesAsync(customerId);
        }

        // ==================== SESSION MANAGEMENT ====================

        public async Task<string?> GetCurrentCustomerIdAsync(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId)) return null;

            return await _storageService.GetCurrentCustomerIdAsync(sessionId);
        }

        public async Task<bool> SetCurrentCustomerAsync(string sessionId, string customerId, bool isStaff)
        {
            if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(customerId))
                return false;

            // Store customer ID in session
            _cache.Set($"session_{sessionId}", customerId, TimeSpan.FromHours(24));

            // Store staff status
            _cache.Set($"session_{sessionId}_staff", isStaff, TimeSpan.FromHours(24));

            return await Task.FromResult(true);
        }

        public async Task<bool> IsStaffSessionAsync(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId)) return false;

            return await _storageService.IsStaffMemberAsync(sessionId);
        }
    }
}
