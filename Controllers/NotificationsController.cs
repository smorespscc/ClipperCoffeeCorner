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

        // =================================
        // === Order Placed Notification ===
        // =================================
        // 1. Gets estimated wait time
        // 2. Sends notification to customer
        // 3. Returns estimated wait time to caller (if UI wants to display it or something)
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

        // ====================================
        // === Order Completed Notification ===
        // ====================================
        // 1. Sends notification to customer
        // -. Could also take care of updating the order, but I am assuming that is handled elsewhere and this just needs to send the notification
        [HttpPost("order-complete")]
        public async Task<IActionResult> OrderComplete([FromBody] int orderId)
        {
            try
            {
                await _service.CompleteOrderAsync(orderId);
                return Ok(new
                {
                    Message = "Customer notified",
                    OrderId = orderId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // ==============================
        // === Get Popular Menu Items ===
        // ==============================
        // 1. gets popular items of a given menu category (if not categoryId provided, gets overall popular items)
        // 2. returns list of popular items with their order counts (list of MenuItemPopularityDto objects)
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
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        OrderItemId = 1,
                        OrderId = 1,
                        CombinationId = 1,
                        Quantity = 1,
                        UnitPrice = 9.99m
                    }
                }
            };

            var fakeUser = new UserResponse
            {
                NotificationPref = notificationPref ?? "Email",
                PhoneNumber = "+18777804236",
                Email = "mamarsh7of9@gmail.com"
            };

            var fakeItemDetails = new List<OrderItemDetailsDto>
            {
                new OrderItemDetailsDto
                {
                    MenuItemId = 1,
                    MenuItemName = "Test Latte",
                    Options = new List<string> { "Oat Milk", "Extra Shot" },
                    Quantity = 1,
                    UnitPrice = 9.99m,
                    LineTotal = 9.99m
                }
            };

            await _service.TestNotificationsAsync(fakeOrder, fakeUser, fakeItemDetails);

            return Ok("Test notification triggered");
        }

        // test wait time estimation
        [HttpPost("test-wait-time")]
        public IActionResult TestWaitTime()
        {
            double estimation = _service.TestWaitTimeEstimation();

            return Ok(estimation);
        }
    }
}