using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using WaitTimeTesting.Models;
using WaitTimeTesting.Services;

namespace WaitTimeTesting.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly WaitTimeNotificationService _service;
        private readonly IOrderStorage _storage;

        public NotificationsController(WaitTimeNotificationService service, IOrderStorage storage)
        {
            _service = service;
            _storage = storage;
        }

        // Example JSON input for order-placed:
        // {
        //   "uid": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        //   "itemIds": "1,3,7,6",
        //   "phoneNumber": "+15551234567",
        //   "notificationPref": 1           // 0=None, 1=Sms
        // }

        [HttpPost("order-placed")]
        public async Task<IActionResult> OrderPlaced([FromBody] Order order)
        {
            try
            {
                await _service.AddOrderAsync(order);
                return Ok(new
                {
                    Message = "Order placed successfully!",
                    OrderId = order.Uid,
                    EstimatedWaitMinutes = Math.Round(order.EstimatedWaitTime ?? 0, 1),
                    PositionInQueue = order.PlaceInQueue
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
                var order = await _service.CompleteOrderAsync(request.Uid, request.CompletedAt);
                return Ok(new
                {
                    Message = "Order completed and customer notified!",
                    OrderId = order.Uid,
                    ActualWaitMinutes = Math.Round((order.CompletedAt!.Value - order.PlacedAt).TotalMinutes, 1)
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // For testing: Trigger retrain manually
        [HttpGet("retrain")]
        public IActionResult Retrain()
        {
            _service.ForceRetrain();
            return Ok("Model retraining triggered.");
        }

        // =====================================================================
        // ================== TESTING AND DEBUGGING ENDPOINTS ==================
        // =====================================================================
        // =====================================================================
        // GetActiveQueue:         returns current active orders in the queue
        // GetPendingTraining:     returns orders pending training evaluation       <- (have to remake this one)
        // GetTrained:             returns orders that have been trained            <- (have to remake this one)
        // PrintQueues:            prints all queues to console/logs for debugging  <- (have to remake this one)
        // PopulateQueue:          populates the queue with test orders

        [HttpGet("queue/active")]
        public IActionResult GetActiveQueue()
        {
            var queue = _service.GetActiveQueue();
            return Ok(queue.OrderBy(o => o.PlacedAt).Select(o => new
            {
                o.Uid,
                Position = _service.GetQueuePosition(o),
                o.ItemIds,
                EstimatedMinutes = Math.Round(o.EstimatedWaitTime ?? 0, 1),
                ItemsAhead = o.ItemsAheadAtPlacement,
                TotalItemsAhead = o.TotalItemsAheadAtPlacement,
                o.PhoneNumber,
                PlacedAgo = $"{(DateTimeOffset.Now - o.PlacedAt).TotalMinutes:F0}m ago"
            }));
        }

        [HttpGet("debug/print")]
        public IActionResult PrintSystemState()
        {
            _service.PrintSystemState();
            return Ok("System state printed to logs.");
        }

        [HttpPost("test/populate-queue")]
        public async Task<IActionResult> PopulateQueue([FromQuery] int count = 10)
        {
            if (count < 1 || count > 50)
                return BadRequest("Count must be 1–50");

            var random = new Random();
            var testPhones = new[] { "+15551234567", "+15557654321", "+15559876543" };
            var menuItems = Enumerable.Range(1, 10).ToList();

            for (int i = 0; i < count; i++)
            {
                var order = new Order
                {
                    Uid = Guid.NewGuid(),
                    ItemIds = string.Join(",", Enumerable.Range(0, random.Next(1, 6))
                        .Select(_ => menuItems[random.Next(menuItems.Count)])),
                    PhoneNumber = testPhones[random.Next(testPhones.Length)],
                    NotificationPref = random.Next(0, 2) == 0 ? NotificationPreference.None : NotificationPreference.Sms
                };

                await _service.AddOrderAsync(order);
            }

            return Ok($"Added {count} test orders. Check /queue/active");
        }
    }

    public class CompleteRequest
    {
        public Guid Uid { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
    }
}