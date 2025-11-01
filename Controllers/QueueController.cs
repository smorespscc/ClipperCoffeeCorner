using Microsoft.AspNetCore.Mvc;

namespace ClipperCoffeeCorner.Controllers
{
    [ApiController]
    [Route("queue")]
    public class QueueController : ControllerBase
    {
        // GET /queue/{orderId}
        [HttpGet("{orderId}")]
        public IActionResult GetQueueStatus(string orderId)
        {
            // later: look up real order/queue
            return Ok(new
            {
                orderId = orderId,
                position = 3,
                estimatedReadyTime = "2025-10-30T14:35:00Z",
                status = "InQueue"   // InQueue, InProgress, Ready, Completed
            });
        }

        // OPTIONAL: GET /queue/current
        [HttpGet("current")]
        public IActionResult GetCurrentQueue()
        {
            return Ok(new[]
            {
                new { orderId = "ord-2025-0001", position = 1, status = "InProgress" },
                new { orderId = "ord-2025-0002", position = 2, status = "InQueue" },
                new { orderId = "ord-2025-0003", position = 3, status = "InQueue" }
            });
        }
    }
}
