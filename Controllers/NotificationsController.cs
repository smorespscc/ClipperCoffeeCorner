using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using ClipperCoffeeCorner.Models;
using ClipperCoffeeCorner.Services;

namespace ClipperCoffeeCorner.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly WaitTimeNotificationService _service;

        public NotificationsController(WaitTimeNotificationService service)
        {
            _service = service;
        }

        [HttpPost("order-placed")]
        public async Task<IActionResult> OrderPlaced([FromBody] Order order)
        {
            try
            {
                var estimatedWaitTime = await _service.ProcessNewOrder(order);
                return Ok(new
                {
                    Message = "Order placed successfully",
                    OrderId = order.OrderId,
                    EstimatedWaitMinutes = estimatedWaitTime,
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("order-complete")]
        public async Task<IActionResult> OrderComplete([FromBody] int orderId)
        {
            try
            {
                var order = await _service.CompleteOrderAsync(orderId);
                return Ok(new
                {
                    Message = "Order completed and customer notified",
                    OrderId = order.OrderId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // gets popular items of a given menu category
        // if no category is given it just gives overall popular items
        // does not work yet
        [HttpGet("popular-items")]
        public async Task<IActionResult> GetPopularItems([FromQuery] int? categoryId = null)
        {
            try
            {
                var items = await _service.GetPopularItemsAsync(categoryId);
                return Ok(items);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // =========================
        // === TESTING ENDPOINTS ===
        // =========================


        // Test Twilio and SendGrid notifications
        [HttpPost("test-notifications")]
        public async Task<IActionResult> TestNotifications(string? notificationPref)
        {
            var fakeOrder = new Order
            {
                OrderId = 1,
                UserId = 1,
                IdempotencyKey = Guid.NewGuid(),
                Status = "Placed",
                PlacedAt = DateTime.UtcNow,
                TotalAmount = 9.99m,
                OrderItems = new List<OrderItem>() 
                {
                    new OrderItem
                    {
                        OrderItemId = 1,
                        OrderId = 1,
                        CombinationId = 1,
                        Quantity = 1,
                        UnitPrice = 9.99m,
                    },
                }
            };

            var fakeUser = new UserResponse
            {
                NotificationPref = notificationPref ?? "Email",
                PhoneNumber = "+18777804236", // Twilio Virtual Phone Number
                Email = "mamarsh7of9@gmail.com" // test email
            };

            await _service.TestNotificationsAsync(fakeOrder, fakeUser);

            return Ok("Test notification triggered");
        }
    }
}