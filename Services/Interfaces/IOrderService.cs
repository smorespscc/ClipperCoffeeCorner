using ClipperCoffeeCorner.Models;
using ClipperCoffeeCorner.Models.Domain;
using ClipperCoffeeCorner.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClipperCoffeeCorner.Services.Interfaces
{
    /// <summary>
    /// Business logic for order management operations.
    /// Handles cart operations, order creation, and order history.
    /// 
    /// This service coordinates between the controller and storage layer,
    /// applying business rules and validation.
    /// </summary>
    public interface IOrderService
    {
        // ==================== CART OPERATIONS ====================
        
        /// <summary>
        /// Adds an item to the cart with validation
        /// </summary>
        Task<CartDto> AddToCartAsync(string sessionId, OrderItem item);
        
        /// <summary>
        /// Removes an item from the cart by index
        /// </summary>
        Task<CartDto> RemoveFromCartAsync(string sessionId, int itemIndex);
        
        /// <summary>
        /// Removes all instances of an item (by name and type)
        /// </summary>
        Task<CartDto> RemoveAllInstancesAsync(string sessionId, string itemName, string itemType);
        
        /// <summary>
        /// Updates an existing cart item
        /// </summary>
        Task<CartDto> UpdateCartItemAsync(string sessionId, int itemIndex, OrderItem updatedItem);
        
        /// <summary>
        /// Retrieves current cart with calculated totals
        /// </summary>
        Task<CartDto> GetCartAsync(string sessionId);
        
        /// <summary>
        /// Clears all items from cart
        /// </summary>
        Task<bool> ClearCartAsync(string sessionId);
        
        /// <summary>
        /// Gets total item count in cart
        /// </summary>
        Task<int> GetCartItemCountAsync(string sessionId);

        // ==================== ORDER CREATION ====================
        
        /// <summary>
        /// Creates an order from current cart
        /// Validates items, calculates totals, applies discounts
        /// </summary>
        Task<Order> CreateOrderFromCartAsync(string sessionId, string customerId);
        
        /// <summary>
        /// Validates order before payment
        /// Checks item availability, pricing, etc.
        /// </summary>
        Task<(bool isValid, string? errorMessage)> ValidateOrderAsync(Order order);
        
        /// <summary>
        /// Processes payment and completes order
        /// </summary>
        Task<(bool success, int? orderNumber, string? errorMessage)> ProcessPaymentAsync(
            Order order, 
            string paymentMethod, 
            string paymentDetails);

        // ==================== ORDER HISTORY ====================
        
        /// <summary>
        /// Retrieves recent orders for a customer
        /// </summary>
        Task<List<Order>> GetRecentOrdersAsync(string customerId, int count = 10);
        
        /// <summary>
        /// Retrieves a specific order by ID
        /// </summary>
        Task<Order?> GetOrderByIdAsync(int orderId);
        
        /// <summary>
        /// Retrieves the last completed order
        /// </summary>
        Task<Order?> GetLastOrderAsync(string customerId);

        // ==================== SAVED ORDERS ====================
        
        /// <summary>
        /// Saves current cart as a favorite order
        /// </summary>
        Task<SavedOrder> SaveCurrentCartAsFavoriteAsync(string sessionId, string customerId, string orderName);
        
        /// <summary>
        /// Applies a saved order to current cart
        /// </summary>
        Task<CartDto> ApplySavedOrderAsync(string sessionId, int savedOrderId);
        
        /// <summary>
        /// Updates an existing saved order
        /// </summary>
        Task<bool> UpdateSavedOrderAsync(int savedOrderId, SavedOrder updatedOrder);
        
        /// <summary>
        /// Deletes a saved order
        /// </summary>
        Task<bool> DeleteSavedOrderAsync(int savedOrderId);
        
        /// <summary>
        /// Retrieves all saved orders for a customer
        /// </summary>
        Task<List<SavedOrder>> GetSavedOrdersAsync(string customerId);

        // ==================== ORDER CALCULATIONS ====================
        
        /// <summary>
        /// Calculates order subtotal (before discounts and tax)
        /// </summary>
        Task<decimal> CalculateSubtotalAsync(List<OrderItem> items);
        
        /// <summary>
        /// Calculates staff discount if applicable
        /// </summary>
        Task<decimal> CalculateStaffDiscountAsync(decimal subtotal, bool isStaff);
        
        /// <summary>
        /// Calculates total with all discounts applied
        /// </summary>
        Task<decimal> CalculateTotalAsync(List<OrderItem> items, bool isStaff);
    }
}
