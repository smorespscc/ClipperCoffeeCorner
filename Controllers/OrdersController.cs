using Microsoft.AspNetCore.Mvc;

namespace ClipperCoffeeCorner.Controllers
{
    [ApiController]
    [Route("orders")]
    public class OrdersController : ControllerBase
    {
        public class OrderItemDto
        {
            public string MenuItemId { get; set; } = "";
            public int Quantity { get; set; }
            public decimal Price { get; set; }

            // drink modifiers
            public string? Style { get; set; }          // hot / iced
            public string? Milk { get; set; }           // dairy / oat
            public string[]? Flavors { get; set; }      // ["hazelnut"]
            public string[]? Additions { get; set; }    // ["extra-shot"]
            public string[]? Restrictions { get; set; } // ["sugar-free"]

            // food
            public string[]? Sauces { get; set; }

            public Dictionary<string, string>? Options { get; set; }
        }

        public class CreateOrderRequest
        {
            public string CustomerId { get; set; } = "";
            public List<OrderItemDto> Items { get; set; } = new();
            public string? Notes { get; set; }
            public string? PaymentMethod { get; set; }
        }

        // POST /orders
        [HttpPost]
        public IActionResult CreateOrder([FromBody] CreateOrderRequest req)
        {
            if (req.Items == null || req.Items.Count == 0)
            {
                return BadRequest(new { error = "NO_ITEMS", message = "Order must contain at least one item." });
            }

            // fake queue position
            return Created($"/orders/ord-2025-0001", new
            {
                orderId = "ord-2025-0001",
                total = req.Items.Sum(i => i.Price * i.Quantity),
                currency = "USD",
                paymentStatus = "Pending",
                queueStatus = new
                {
                    position = 4,
                    status = "InQueue"
                },
                items = req.Items
            });
        }
    }
}
