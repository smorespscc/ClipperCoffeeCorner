using Microsoft.AspNetCore.Mvc;

namespace ClipperCoffeeCorner.Controllers
{
    // This controller is for the UI team (the "Submit Button" issue)
    [ApiController]
    [Route("ui")]
    public class UiController : ControllerBase
    {
        // ---- request DTOs ----
        public class SubmitItemDto
        {
            public string MenuItemId { get; set; } = "";   // e.g. "lotus" or "chicken-strips"
            public int Quantity { get; set; }              // e.g. 1
            public decimal Price { get; set; }             // UI price; server can re-check

            // drink modifiers (come from GET /menu)
            public string? Style { get; set; }             // "iced", "hot"
            public string? Milk { get; set; }              // "oat", "dairy"
            public string[]? Flavors { get; set; }         // ["hazelnut"]
            public string[]? Additions { get; set; }       // ["extra-shot"]
            public string[]? Restrictions { get; set; }    // ["sugar-free"]

            // extra key/value options
            public Dictionary<string, string>? Options { get; set; }
        }

        public class SubmitOrderRequest
        {
            // if UI already saved a draft order on the server
            public string? DraftOrderId { get; set; }      // "draft-123"

            // who is placing the order (from /auth/login)
            public string CustomerId { get; set; } = "";   // "u-9283"

            // where this order comes from (for logging / reporting)
            public string SubmitSource { get; set; } = "Kiosk";  // Kiosk, Web, Mobile

            // if the UI didn't save draft and just wants to send items now
            public List<SubmitItemDto>? Items { get; set; }

            public string? Notes { get; set; }             // "Make it fast"
            public string? PaymentMethod { get; set; }     // "Card", "Cash", "Campus"
        }

        // ---- endpoint ----

        // POST /ui/submit
        [HttpPost("submit")]
        public IActionResult SubmitOrder([FromBody] SubmitOrderRequest req)
        {
            // 1. Basic validation:
            // either we have a draft id OR we have items. If both missing -> 400.
            if ((req.Items == null || req.Items.Count == 0) && string.IsNullOrWhiteSpace(req.DraftOrderId))
            {
                return BadRequest(new
                {
                    error = "NO_ORDER_DATA",
                    message = "Provide either draftOrderId or items."
                });
            }

            // 2. (later) you would:
            // - look up draft
            // - validate menu item ids
            // - calc real total
            // - create order in DB
            // - send to queue / notification
            // For now we just fake a success:

            return Ok(new
            {
                orderId = "ord-2025-0001",
                status = "Submitted",
                payment = new
                {
                    status = "Pending",
                    paymentUrl = "https://pay.example.com/session/abc"
                },
                queue = new
                {
                    position = 5,
                    status = "InQueue"
                },
                items = req.Items
            });
        }
    }
}
