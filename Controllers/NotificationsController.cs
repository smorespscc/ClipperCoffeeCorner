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

        // Example JSON input for order-placed:
        // {
        //   "OrderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        //   "LineItems": "1,3,7,6",
        //   "phoneNumber": "+15551234567",
        //   "notificationPref": 1           // 0=None, 1=Sms, 2=Email
        // }

        [HttpPost("order-placed")]
        public async Task<IActionResult> OrderPlaced([FromBody] Order order)
        {
            try
            {
                var estimatedWaitTime = await _service.AddOrderAsync(order);
                return Ok(new
                {
                    Message = "Order placed successfully!",
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
        public async Task<IActionResult> OrderComplete([FromBody] CompleteRequest request)
        {
            try
            {
                var order = await _service.CompleteOrderAsync(request.OrderId);
                return Ok(new
                {
                    Message = "Order completed and customer notified!",
                    OrderId = order.OrderId,
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
    public class CompleteRequest
    {
        public Guid OrderId { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
    }
}