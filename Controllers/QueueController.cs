using ClipperCoffeeCorner.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ClipperCoffeeCorner.Controllers
{
    /// <summary>
    /// API Controller for queue operations.
    /// Handles queue display, position tracking, and wait time estimation.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class QueueController : ControllerBase
    {
        private readonly IQueueService _queueService;
        private readonly ILogger<QueueController> _logger;

        public QueueController(
            IQueueService queueService,
            ILogger<QueueController> logger)
        {
            _queueService = queueService ?? throw new ArgumentNullException(nameof(queueService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ==================== QUEUE DISPLAY ====================

        /// <summary>
        /// Gets current queue
        /// GET /api/queue/current
        /// </summary>
        [HttpGet("current")]
        public async Task<ActionResult> GetCurrentQueue()
        {
            try
            {
                var queue = await _queueService.GetCurrentQueueAsync();
                return Ok(new { queue });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current queue");
                return StatusCode(500, new { message = "An error occurred while retrieving queue" });
            }
        }

        /// <summary>
        /// Gets placeholder queue for demonstration
        /// GET /api/queue/placeholder
        /// </summary>
        [HttpGet("placeholder")]
        public async Task<ActionResult> GetPlaceholderQueue([FromQuery] int baseOrderNumber = 200)
        {
            try
            {
                var queue = await _queueService.GeneratePlaceholderQueueAsync(baseOrderNumber);
                return Ok(new { queue });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating placeholder queue");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        // ==================== QUEUE POSITION ====================

        /// <summary>
        /// Gets customer's position in queue
        /// GET /api/queue/position/{orderNumber}
        /// </summary>
        [HttpGet("position/{orderNumber}")]
        public async Task<ActionResult> GetQueuePosition(int orderNumber)
        {
            try
            {
                var position = await _queueService.GetCustomerQueuePositionAsync(orderNumber);
                
                if (position == null)
                {
                    return NotFound(new { message = "Order not found in queue" });
                }

                return Ok(new { orderNumber, position });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting queue position");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        // ==================== WAIT TIME ====================

        /// <summary>
        /// Gets estimated wait time for an order
        /// GET /api/queue/wait-time/{orderNumber}
        /// </summary>
        [HttpGet("wait-time/{orderNumber}")]
        public async Task<ActionResult> GetWaitTime(int orderNumber)
        {
            try
            {
                var waitTime = await _queueService.EstimateWaitTimeAsync(orderNumber);
                return Ok(new { orderNumber, waitTime });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting wait time");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        // ==================== QUEUE MANAGEMENT ====================

        /// <summary>
        /// Adds order to queue
        /// POST /api/queue/add
        /// </summary>
        [HttpPost("add")]
        public async Task<ActionResult> AddToQueue([FromBody] AddToQueueRequest request)
        {
            try
            {
                var success = await _queueService.AddToQueueAsync(request.OrderNumber, request.ItemDescription);
                
                if (!success)
                {
                    return BadRequest(new { message = "Order already in queue" });
                }

                return Ok(new { message = "Order added to queue successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding to queue");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        /// <summary>
        /// Updates order status in queue
        /// PUT /api/queue/status
        /// </summary>
        [HttpPut("status")]
        public async Task<ActionResult> UpdateStatus([FromBody] UpdateStatusRequest request)
        {
            try
            {
                var success = await _queueService.UpdateOrderStatusAsync(request.OrderNumber, request.Status);
                
                if (!success)
                {
                    return NotFound(new { message = "Order not found in queue" });
                }

                return Ok(new { message = "Status updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        /// <summary>
        /// Checks if order is ready
        /// GET /api/queue/is-ready/{orderNumber}
        /// </summary>
        [HttpGet("is-ready/{orderNumber}")]
        public async Task<ActionResult> IsOrderReady(int orderNumber)
        {
            try
            {
                var isReady = await _queueService.IsOrderReadyAsync(orderNumber);
                return Ok(new { orderNumber, isReady });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if order is ready");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }
    }

    // ==================== REQUEST MODELS ====================

    public class AddToQueueRequest
    {
        public int OrderNumber { get; set; }
        public required string ItemDescription { get; set; }
    }

    public class UpdateStatusRequest
    {
        public int OrderNumber { get; set; }
        public QueueStatus Status { get; set; }
    }
}
