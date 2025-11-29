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

        // POST: api/notifications/order-placed/1
        [HttpPost("order-placed/{orderId}")]
        public async Task<IActionResult> OrderPlaced(int orderId)
        {
            try
            {
                var estimatedWaitTime = await _service.ProcessNewOrder(orderId);
                return Ok(new
                {
                    Message = "Order placed successfully",
                    OrderId = orderId,
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

        // POST: api/notifications/order-complete/1
        [HttpPost("order-complete/{orderId}")]
        public async Task<IActionResult> OrderComplete(int orderId)
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

        // GET: api/notifications/popular-items?categoryId=2
        [HttpGet("popular-items")]
        public async Task<IActionResult> GetPopularItems(int? categoryId = null)
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
        // POST: api/notifications/test-notifications?notificationPref=Email
        [HttpPost("test-notifications")]
        public async Task<IActionResult> TestNotifications(string notificationPref = "Email")
        {
            await _service.TestNotificationsAsync(notificationPref);

            return Ok($"Test {notificationPref} notification triggered");
        }

        // test wait time estimation
        // POST: api/notifications/test-wait-time
        [HttpPost("test-wait-time")]
        public IActionResult TestWaitTime()
        {
            double estimation = _service.TestWaitTimeEstimation();

            return Ok(estimation);
        }
    }
}