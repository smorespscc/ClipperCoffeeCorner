using ClipperCoffeeCorner.Models;
using ClipperCoffeeCorner.Models.Domain;
using ClipperCoffeeCorner.Models.DTOs;
using ClipperCoffeeCorner.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ClipperCoffeeCorner.Controllers
{
    /// <summary>
    /// API Controller for order and cart operations.
    /// Handles cart management, order creation, and saved orders.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderService orderService,
            ICustomerService customerService,
            ILogger<OrderController> logger)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ==================== CART OPERATIONS ====================

        /// <summary>
        /// Adds an item to the cart
        /// POST /api/order/cart/add
        /// </summary>
        [HttpPost("cart/add")]
        public async Task<ActionResult<CartDto>> AddToCart([FromBody] OrderItem item)
        {
            try
            {
                var sessionId = HttpContext.Session.Id;
                var cart = await _orderService.AddToCartAsync(sessionId, item);
                return Ok(cart);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid item data");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart");
                return StatusCode(500, new { message = "An error occurred while adding item to cart" });
            }
        }

        /// <summary>
        /// Removes an item from cart by index
        /// DELETE /api/order/cart/remove/{index}
        /// </summary>
        [HttpDelete("cart/remove/{index}")]
        public async Task<ActionResult<CartDto>> RemoveFromCart(int index)
        {
            try
            {
                var sessionId = HttpContext.Session.Id;
                var cart = await _orderService.RemoveFromCartAsync(sessionId, index);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cart");
                return StatusCode(500, new { message = "An error occurred while removing item" });
            }
        }

        /// <summary>
        /// Removes all instances of an item
        /// DELETE /api/order/cart/remove-all
        /// </summary>
        [HttpDelete("cart/remove-all")]
        public async Task<ActionResult<CartDto>> RemoveAllInstances([FromQuery] string itemName, [FromQuery] string itemType)
        {
            try
            {
                var sessionId = HttpContext.Session.Id;
                var cart = await _orderService.RemoveAllInstancesAsync(sessionId, itemName, itemType);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing items from cart");
                return StatusCode(500, new { message = "An error occurred while removing items" });
            }
        }

        /// <summary>
        /// Gets current cart
        /// GET /api/order/cart
        /// </summary>
        [HttpGet("cart")]
        public async Task<ActionResult<CartDto>> GetCart()
        {
            try
            {
                var sessionId = HttpContext.Session.Id;
                var cart = await _orderService.GetCartAsync(sessionId);
                return Ok(cart);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting cart");
                return StatusCode(500, new { message = "An error occurred while retrieving cart" });
            }
        }

        /// <summary>
        /// Clears all items from cart
        /// DELETE /api/order/cart/clear
        /// </summary>
        [HttpDelete("cart/clear")]
        public async Task<ActionResult> ClearCart()
        {
            try
            {
                var sessionId = HttpContext.Session.Id;
                await _orderService.ClearCartAsync(sessionId);
                return Ok(new { message = "Cart cleared successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                return StatusCode(500, new { message = "An error occurred while clearing cart" });
            }
        }

        // ==================== ORDER CREATION ====================

        /// <summary>
        /// Creates an order from current cart
        /// POST /api/order/create
        /// </summary>
        [HttpPost("create")]
        public async Task<ActionResult<Order>> CreateOrder()
        {
            try
            {
                var sessionId = HttpContext.Session.Id;
                var customerId = await _customerService.GetCurrentCustomerIdAsync(sessionId) ?? "guest";

                var order = await _orderService.CreateOrderFromCartAsync(sessionId, customerId);
                return Ok(order);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot create order");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, new { message = "An error occurred while creating order" });
            }
        }

        /// <summary>
        /// Processes payment for an order
        /// POST /api/order/payment
        /// </summary>
        [HttpPost("payment")]
        public async Task<ActionResult> ProcessPayment([FromBody] PaymentRequest request)
        {
            try
            {
                var sessionId = HttpContext.Session.Id;
                var customerId = await _customerService.GetCurrentCustomerIdAsync(sessionId) ?? "guest";

                // Create order from cart
                var order = await _orderService.CreateOrderFromCartAsync(sessionId, customerId);

                // Process payment
                var (success, orderNumber, errorMessage) = await _orderService.ProcessPaymentAsync(
                    order,
                    request.PaymentMethod,
                    request.PaymentDetails);

                if (!success)
                {
                    return BadRequest(new { message = errorMessage });
                }

                // Clear cart after successful payment
                await _orderService.ClearCartAsync(sessionId);

                return Ok(new
                {
                    success = true,
                    orderNumber = orderNumber,
                    message = "Payment processed successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment");
                return StatusCode(500, new { message = "An error occurred while processing payment" });
            }
        }

        // ==================== SAVED ORDERS ====================

        /// <summary>
        /// Gets all saved orders for current customer
        /// GET /api/order/saved
        /// </summary>
        [HttpGet("saved")]
        public async Task<ActionResult> GetSavedOrders()
        {
            try
            {
                var sessionId = HttpContext.Session.Id;
                var customerId = await _customerService.GetCurrentCustomerIdAsync(sessionId);

                if (string.IsNullOrEmpty(customerId))
                {
                    return Ok(new { orders = new object[] { } });
                }

                var orders = await _orderService.GetSavedOrdersAsync(customerId);
                return Ok(new { orders });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting saved orders");
                return StatusCode(500, new { message = "An error occurred while retrieving saved orders" });
            }
        }

        /// <summary>
        /// Saves current cart as a favorite order
        /// POST /api/order/saved
        /// </summary>
        [HttpPost("saved")]
        public async Task<ActionResult> SaveCurrentCart([FromBody] SaveOrderRequest request)
        {
            try
            {
                var sessionId = HttpContext.Session.Id;
                var customerId = await _customerService.GetCurrentCustomerIdAsync(sessionId);

                if (string.IsNullOrEmpty(customerId))
                {
                    return Unauthorized(new { message = "Please login to save orders" });
                }

                var savedOrder = await _orderService.SaveCurrentCartAsFavoriteAsync(
                    sessionId,
                    customerId,
                    request.OrderName);

                return Ok(new { savedOrder, message = "Order saved successfully" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot save order");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving order");
                return StatusCode(500, new { message = "An error occurred while saving order" });
            }
        }

        /// <summary>
        /// Applies a saved order to cart
        /// POST /api/order/saved/{id}/apply
        /// </summary>
        [HttpPost("saved/{id}/apply")]
        public async Task<ActionResult<CartDto>> ApplySavedOrder(int id)
        {
            try
            {
                var sessionId = HttpContext.Session.Id;
                var cart = await _orderService.ApplySavedOrderAsync(sessionId, id);
                return Ok(cart);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Cannot apply saved order");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying saved order");
                return StatusCode(500, new { message = "An error occurred while applying saved order" });
            }
        }

        /// <summary>
        /// Deletes a saved order
        /// DELETE /api/order/saved/{id}
        /// </summary>
        [HttpDelete("saved/{id}")]
        public async Task<ActionResult> DeleteSavedOrder(int id)
        {
            try
            {
                await _orderService.DeleteSavedOrderAsync(id);
                return Ok(new { message = "Saved order deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting saved order");
                return StatusCode(500, new { message = "An error occurred while deleting saved order" });
            }
        }

        // ==================== RECENT ORDERS ====================

        /// <summary>
        /// Gets recent orders for current customer
        /// GET /api/order/recent
        /// </summary>
        [HttpGet("recent")]
        public async Task<ActionResult> GetRecentOrders()
        {
            try
            {
                var sessionId = HttpContext.Session.Id;
                var customerId = await _customerService.GetCurrentCustomerIdAsync(sessionId);

                if (string.IsNullOrEmpty(customerId))
                {
                    return Ok(new { orders = new object[] { } });
                }

                var orders = await _orderService.GetRecentOrdersAsync(customerId);
                return Ok(new { orders });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recent orders");
                return StatusCode(500, new { message = "An error occurred while retrieving recent orders" });
            }
        }
    }

    // ==================== REQUEST MODELS ====================

    public class PaymentRequest
    {
        public required string PaymentMethod { get; set; }
        public required string PaymentDetails { get; set; }
    }

    public class SaveOrderRequest
    {
        public required string OrderName { get; set; }
    }
}
