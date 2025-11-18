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
        public async Task<IActionResult> OrderComplete([FromBody] Guid orderId)
        {
            try
            {
                var order = await _service.CompleteOrderAsync(orderId);
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
}