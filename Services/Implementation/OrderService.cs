using ClipperCoffeeCorner.Models;
using ClipperCoffeeCorner.Models.Domain;
using ClipperCoffeeCorner.Models.DTOs;
using ClipperCoffeeCorner.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClipperCoffeeCorner.Services.Implementation
{
    /// <summary>
    /// Implementation of order management business logic.
    /// Handles cart operations, order creation, and saved orders.
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly IStorageService _storageService;
        private readonly IPricingService _pricingService;

        public OrderService(IStorageService storageService, IPricingService pricingService)
        {
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _pricingService = pricingService ?? throw new ArgumentNullException(nameof(pricingService));
        }

        // ==================== CART OPERATIONS ====================

        public async Task<CartDto> AddToCartAsync(string sessionId, OrderItem item)
        {
            if (string.IsNullOrEmpty(sessionId)) throw new ArgumentNullException(nameof(sessionId));
            if (item == null) throw new ArgumentNullException(nameof(item));

            // Validate item
            if (string.IsNullOrEmpty(item.Name))
                throw new ArgumentException("Item name is required");

            // Calculate prices
            await _pricingService.UpdateOrderItemPricesAsync(item);

            // Get existing cart
            var items = await _storageService.GetCartItemsAsync(sessionId);

            // Add item
            items.Add(item);

            // Save cart
            await _storageService.SaveCartItemsAsync(sessionId, items);

            // Return cart DTO
            return await GetCartAsync(sessionId);
        }

        public async Task<CartDto> RemoveFromCartAsync(string sessionId, int itemIndex)
        {
            if (string.IsNullOrEmpty(sessionId)) throw new ArgumentNullException(nameof(sessionId));

            var items = await _storageService.GetCartItemsAsync(sessionId);

            if (itemIndex >= 0 && itemIndex < items.Count)
            {
                items.RemoveAt(itemIndex);
                await _storageService.SaveCartItemsAsync(sessionId, items);
            }

            return await GetCartAsync(sessionId);
        }

        public async Task<CartDto> RemoveAllInstancesAsync(string sessionId, string itemName, string itemType)
        {
            if (string.IsNullOrEmpty(sessionId)) throw new ArgumentNullException(nameof(sessionId));

            var items = await _storageService.GetCartItemsAsync(sessionId);
            items.RemoveAll(i => i.Name == itemName && i.Type == itemType);
            await _storageService.SaveCartItemsAsync(sessionId, items);

            return await GetCartAsync(sessionId);
        }

        public async Task<CartDto> UpdateCartItemAsync(string sessionId, int itemIndex, OrderItem updatedItem)
        {
            if (string.IsNullOrEmpty(sessionId)) throw new ArgumentNullException(nameof(sessionId));
            if (updatedItem == null) throw new ArgumentNullException(nameof(updatedItem));

            var items = await _storageService.GetCartItemsAsync(sessionId);

            if (itemIndex >= 0 && itemIndex < items.Count)
            {
                await _pricingService.UpdateOrderItemPricesAsync(updatedItem);
                items[itemIndex] = updatedItem;
                await _storageService.SaveCartItemsAsync(sessionId, items);
            }

            return await GetCartAsync(sessionId);
        }

        public async Task<CartDto> GetCartAsync(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId)) throw new ArgumentNullException(nameof(sessionId));

            var items = await _storageService.GetCartItemsAsync(sessionId);
            var isStaff = await _storageService.IsStaffMemberAsync(sessionId);
            var specialRequests = await _storageService.GetSpecialRequestsAsync(sessionId);

            var cart = new CartDto
            {
                Items = items,
                SessionId = sessionId,
                SpecialRequests = specialRequests,
                Success = true
            };

            cart.CalculateTotals(isStaff);

            return cart;
        }

        public async Task<bool> ClearCartAsync(string sessionId)
        {
            if (string.IsNullOrEmpty(sessionId)) return false;
            return await _storageService.ClearCartAsync(sessionId);
        }

        public async Task<int> GetCartItemCountAsync(string sessionId)
        {
            var items = await _storageService.GetCartItemsAsync(sessionId);
            return items.Sum(i => i.Quantity);
        }

        // ==================== ORDER CREATION ====================

        public async Task<Order> CreateOrderFromCartAsync(string sessionId, string customerId)
        {
            if (string.IsNullOrEmpty(sessionId)) throw new ArgumentNullException(nameof(sessionId));

            var items = await _storageService.GetCartItemsAsync(sessionId);
            if (items.Count == 0)
                throw new InvalidOperationException("Cannot create order from empty cart");

            var isStaff = await _storageService.IsStaffMemberAsync(sessionId);
            var (subtotal, discount, total) = await _pricingService.CalculateOrderTotalsAsync(items, isStaff);

            var order = new Order
            {
                IdempotencyKey = Guid.NewGuid().ToString(),
                CustomerId = customerId,
                Currency = "USD",
                LineItems = items.Select(item => new LineItem
                {
                    Name = item.Name,
                    BasePriceMoney = new Money { Amount = (long)(item.BasePrice * 100), Currency = "USD" },
                    Quantity = item.Quantity.ToString()
                }).ToList(),
                SubtotalMoney = (long)(subtotal * 100),
                TotalDiscountMoney = (long)(discount * 100),
                TotalMoney = (long)(total * 100),
                CreatedAt = DateTimeOffset.UtcNow,
                Status = OrderStatus.Open
            };

            return order;
        }

        public Task<(bool isValid, string? errorMessage)> ValidateOrderAsync(Order order)
        {
            if (order == null)
                return Task.FromResult((false, "Order is null"));

            if (order.LineItems == null || order.LineItems.Count == 0)
                return Task.FromResult((false, "Order has no items"));

            if (order.TotalMoney <= 0)
                return Task.FromResult((false, "Order total must be greater than zero"));

            return Task.FromResult((true, (string?)null));
        }

        public async Task<(bool success, int? orderNumber, string? errorMessage)> ProcessPaymentAsync(
            Order order,
            string paymentMethod,
            string paymentDetails)
        {
            // Validate order
            var (isValid, errorMessage) = await ValidateOrderAsync(order);
            if (!isValid)
                return (false, null, errorMessage);

            // In production, integrate with payment gateway (Square, Stripe, etc.)
            // For now, simulate payment processing
            var random = new Random();
            var success = random.Next(0, 10) > 2; // 80% success rate for demo

            if (!success)
                return (false, null, "Payment processing failed. Please try again.");

            // Save order
            order.PlacedAt = DateTimeOffset.UtcNow;
            order.Status = OrderStatus.Placed;
            var orderNumber = await _storageService.SaveOrderAsync(order);

            // Add to recent orders
            if (!string.IsNullOrEmpty(order.CustomerId))
            {
                await _storageService.AddToRecentOrdersAsync(order.CustomerId, order);
            }

            return (true, orderNumber, null);
        }

        // ==================== ORDER HISTORY ====================

        public async Task<List<Order>> GetRecentOrdersAsync(string customerId, int count = 10)
        {
            if (string.IsNullOrEmpty(customerId)) return new List<Order>();

            var orders = await _storageService.GetRecentOrdersAsync(customerId);
            return orders.Take(count).ToList();
        }

        public Task<Order?> GetOrderByIdAsync(int orderId)
        {
            // In production, query database by order ID
            // For now, return null (not implemented in cache)
            return Task.FromResult<Order?>(null);
        }

        public async Task<Order?> GetLastOrderAsync(string customerId)
        {
            if (string.IsNullOrEmpty(customerId)) return null;
            return await _storageService.GetLastOrderAsync(customerId);
        }

        // ==================== SAVED ORDERS ====================

        public async Task<SavedOrder> SaveCurrentCartAsFavoriteAsync(string sessionId, string customerId, string orderName)
        {
            if (string.IsNullOrEmpty(sessionId)) throw new ArgumentNullException(nameof(sessionId));
            if (string.IsNullOrEmpty(customerId)) throw new ArgumentNullException(nameof(customerId));
            if (string.IsNullOrEmpty(orderName)) throw new ArgumentNullException(nameof(orderName));

            var items = await _storageService.GetCartItemsAsync(sessionId);
            if (items.Count == 0)
                throw new InvalidOperationException("Cannot save empty cart");

            // Group items by name/type to create tabs
            var firstItem = items[0];
            var savedOrder = new SavedOrder
            {
                CustomerId = customerId,
                Name = orderName,
                ItemName = firstItem.Name,
                ItemType = firstItem.Type,
                Tabs = items.Select((item, index) => new SavedOrderTab
                {
                    Modifiers = item.Modifiers,
                    SpecialRequests = item.SpecialRequests,
                    TabOrder = index
                }).ToList(),
                SavedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            savedOrder.Id = await _storageService.SaveFavoriteOrderAsync(savedOrder);
            return savedOrder;
        }

        public async Task<CartDto> ApplySavedOrderAsync(string sessionId, int savedOrderId)
        {
            if (string.IsNullOrEmpty(sessionId)) throw new ArgumentNullException(nameof(sessionId));

            var customerId = await _storageService.GetCurrentCustomerIdAsync(sessionId);
            if (string.IsNullOrEmpty(customerId))
                throw new InvalidOperationException("Customer not found");

            var savedOrders = await _storageService.GetSavedOrdersAsync(customerId);
            var savedOrder = savedOrders.FirstOrDefault(o => o.Id == savedOrderId);

            if (savedOrder == null)
                throw new InvalidOperationException("Saved order not found");

            // Get current cart
            var items = await _storageService.GetCartItemsAsync(sessionId);

            // Convert saved order tabs to order items
            var modifierPrices = await _pricingService.GetAllModifierPricesAsync();
            var basePrice = await _pricingService.GetBasePriceAsync(savedOrder.ItemName);

            foreach (var tab in savedOrder.Tabs)
            {
                var orderItem = new OrderItem
                {
                    Name = savedOrder.ItemName,
                    Type = savedOrder.ItemType,
                    BasePrice = basePrice,
                    Quantity = 1,
                    Modifiers = new List<string>(tab.Modifiers),
                    SpecialRequests = tab.SpecialRequests,
                    FromSavedOrderId = savedOrderId
                };

                await _pricingService.UpdateOrderItemPricesAsync(orderItem);
                items.Add(orderItem);
            }

            await _storageService.SaveCartItemsAsync(sessionId, items);
            return await GetCartAsync(sessionId);
        }

        public async Task<bool> UpdateSavedOrderAsync(int savedOrderId, SavedOrder updatedOrder)
        {
            if (updatedOrder == null) throw new ArgumentNullException(nameof(updatedOrder));

            updatedOrder.Id = savedOrderId;
            updatedOrder.UpdatedAt = DateTime.UtcNow;

            return await _storageService.UpdateSavedOrderAsync(updatedOrder);
        }

        public async Task<bool> DeleteSavedOrderAsync(int savedOrderId)
        {
            return await _storageService.DeleteSavedOrderAsync(savedOrderId);
        }

        public async Task<List<SavedOrder>> GetSavedOrdersAsync(string customerId)
        {
            if (string.IsNullOrEmpty(customerId)) return new List<SavedOrder>();
            return await _storageService.GetSavedOrdersAsync(customerId);
        }

        // ==================== ORDER CALCULATIONS ====================

        public async Task<decimal> CalculateSubtotalAsync(List<OrderItem> items)
        {
            return await _pricingService.CalculateSubtotalAsync(items);
        }

        public async Task<decimal> CalculateStaffDiscountAsync(decimal subtotal, bool isStaff)
        {
            return await _pricingService.CalculateStaffDiscountAsync(subtotal, isStaff);
        }

        public async Task<decimal> CalculateTotalAsync(List<OrderItem> items, bool isStaff)
        {
            return await _pricingService.CalculateTotalAsync(items, isStaff);
        }
    }
}
