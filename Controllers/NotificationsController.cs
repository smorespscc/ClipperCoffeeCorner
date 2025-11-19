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

        // this depends on DB so will be changed later
        // gets popular items of a given menu category
        // maybe if no category is given it just gives overall popular items
        [HttpGet("get-popular-items")]
        public async Task<IActionResult> GetPopularItems([FromBody] Guid MenuCategory)
        {
            try
            {
                var popularItems = await _service.GetPopularItemsAsync(MenuCategory);
                return Ok(new
                {
                    Message = "Popular items retrieved successfully!",
                    Items = popularItems.Select(item => new
                    {
                        item.MenuItemId, // id of items
                        item.Name,       // name of items
                        item.OrderCount  // number of times item was ordered (how popular it is)
                    })
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}