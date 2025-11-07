using Microsoft.AspNetCore.Mvc;
using System;
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
                order.PlacedAt = DateTimeOffset.Now; 
                order.Status = OrderStatus.Pending;
                order.PlaceInQueue = _service.GetCurrentQueueLength() + 1;
                var (waitTime, features) = _service.EstimateWaitTime(order);
                order.EstimatedWaitTime = waitTime;
                order.ItemsAheadAtPlacement = features.ItemsAhead;
                order.TotalItemsAheadAtPlacement = features.TotalItemsAhead;
                _service.AddOrder(order);
                await _service.SendNotificationAsync(order, NotificationType.Placement);
                return Ok(new { Message = "Order placed", EstimatedWaitTime = order.EstimatedWaitTime });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("order-complete")]
        public async Task<IActionResult> OrderComplete([FromBody] CompleteRequest request)
        {
            try
            {
                var order = _service.RemoveOrder(request.Uid);
                order.CompletedAt = request.CompletedAt ?? DateTimeOffset.Now;
                order.Status = OrderStatus.Complete;
                await _service.SendNotificationAsync(order, NotificationType.Completion);
                _storage.StoreCompleted(order);
                _service.EvaluateAndLogForRetraining(order);
                return Ok("Order completed");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // For testing: Trigger retrain manually
        [HttpGet("retrain")]
        public IActionResult Retrain()
        {
            _service.RetrainModel();
            return Ok("Model retrained");
        }

        // =====================================================================
        // ================== TESTING AND DEBUGGING ENDPOINTS ==================
        // =====================================================================
        // =====================================================================
        // GetActiveQueue:         returns current active orders in the queue
        // GetPendingTraining:     returns orders pending training evaluation
        // GetTrained:             returns orders that have been trained
        // PrintQueues:            prints all queues to console/logs for debugging
        // PopulateQueue:          populates the queue with test orders

        [HttpGet("queue/active")]
        public IActionResult GetActiveQueue()
        {
            var queue = _service.GetActiveQueue();
            return Ok(queue.Select(o => new
            {
                o.Uid,
                Position = o.PlaceInQueue,
                o.ItemIds,
                EstimatedMinutes = o.EstimatedWaitTime?.TotalMinutes,
                o.ItemsAheadAtPlacement,
                o.TotalItemsAheadAtPlacement,
                o.PhoneNumber
            }));
        }

        [HttpGet("queue/pending-training")]
        public IActionResult GetPendingTraining()
        {
            var pending = _service.GetPendingTrainingOrders();
            return Ok(pending.Select(o => new
            {
                o.Uid,
                o.ItemIds,
                ActualMinutes = (o.CompletedAt.GetValueOrDefault() - o.PlacedAt).TotalMinutes,
                EstimatedMinutes = o.EstimatedWaitTime?.TotalMinutes,
                o.ItemsAheadAtPlacement,
                o.TotalItemsAheadAtPlacement,
                Error = Math.Abs((o.CompletedAt.GetValueOrDefault() - o.PlacedAt).TotalMinutes - o.EstimatedWaitTime!.Value.TotalMinutes)
            }));
        }

        [HttpGet("queue/trained")]
        public IActionResult GetTrained()
        {
            return Ok(_service.GetTrainedOrders().Select(o => new { o.Uid, o.ItemIds }));
        }

        [HttpGet("debug/print")]
        public IActionResult PrintQueues()
        {
            _service.PrintAllQueues();
            return Ok("Queues printed to console/logs");
        }

        [HttpPost("test/populate-queue")]
        public async Task<IActionResult> PopulateQueue([FromQuery] int count = 10)
        {
            if (count < 1 || count > 50)
                return BadRequest("Count must be 1–50");

            var random = new Random();
            var testPhoneNumbers = new[]
            {
        "+15551234567", // Replace with your real test number(s)
        "+15557654321",
        "+15559876543"
    };

            var menuItems = Enumerable.Range(1, 10).ToList(); // IDs 1–10

            for (int i = 0; i < count; i++)
            {
                var order = new Order
                {
                    Uid = Guid.NewGuid(),
                    ItemIds = string.Join(",",
                        Enumerable.Range(0, random.Next(1, 6))
                                  .Select(_ => menuItems[random.Next(menuItems.Count)])),
                    PhoneNumber = testPhoneNumbers[random.Next(testPhoneNumbers.Length)],
                    NotificationPref = random.Next(0, 2) switch
                    {
                        0 => NotificationPreference.None,
                        1 => NotificationPreference.Sms,
                        _ => throw new NotImplementedException(),
                    }
                };

                order.PlacedAt = DateTimeOffset.Now.AddSeconds(-random.Next(0, 1200)); // Spread over last 20 min
                order.Status = OrderStatus.Pending;
                order.PlaceInQueue = _service.GetCurrentQueueLength() + 1;

                _service.AddOrder(order);

                var (waitTime, features) = _service.EstimateWaitTime(order);
                order.EstimatedWaitTime = waitTime;
                order.ItemsAheadAtPlacement = features.ItemsAhead;
                order.TotalItemsAheadAtPlacement = features.TotalItemsAhead;

                if (order.NotificationPref == NotificationPreference.Sms)
                    await _service.SendNotificationAsync(order, NotificationType.Placement);
            }

            return Ok($"Added {count} test orders.");
        }
    }

    public class CompleteRequest
    {
        public Guid Uid { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
    }
}