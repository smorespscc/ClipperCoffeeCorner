using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClipperCoffeeCorner.Data;
using ClipperCoffeeCorner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClipperCoffeeCorner.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _db;

        public OrdersController(AppDbContext db)
        {
            _db = db;
        }

        // DTOs
        public record OrderItemDto(int CombinationId, int Quantity);

        public record CreateOrderRequest(
            int? UserId,                 // null = guest
            List<OrderItemDto> Items);

        public record OrderSummaryDto(
            int OrderId,
            string Status,
            DateTime PlacedAt,
            DateTime? CompletedAt,
            decimal TotalAmount);

        public record UpdateStatusRequest(string Status);

        // POST: api/orders
        [HttpPost]
        public async Task<ActionResult<OrderSummaryDto>> CreateOrder(
            [FromBody] CreateOrderRequest req)
        {
            if (req.Items == null || req.Items.Count == 0)
                return BadRequest("Order must contain at least one item.");

            var comboIds = req.Items.Select(i => i.CombinationId).Distinct().ToList();

            var combos = await _db.Combinations
                .Where(c => comboIds.Contains(c.CombinationId) && c.IsActive)
                .ToDictionaryAsync(c => c.CombinationId);

            decimal total = 0m;
            var orderItems = new List<OrderItem>();

            foreach (var line in req.Items)
            {
                if (!combos.TryGetValue(line.CombinationId, out var combo))
                {
                    return BadRequest($"Invalid CombinationId: {line.CombinationId}");
                }

                var unitPrice = combo.Price;
                var lineTotal = unitPrice * line.Quantity;
                total += lineTotal;

                orderItems.Add(new OrderItem
                {
                    CombinationId = line.CombinationId,
                    Quantity = line.Quantity,
                    UnitPrice = unitPrice
                    // LineTotal is computed in SQL, not set here
                });
            }

            var order = new Order
            {
                UserId = req.UserId,
                IdempotencyKey = Guid.NewGuid(),
                Status = "Pending",
                PlacedAt = DateTime.UtcNow,
                TotalAmount = total,
                OrderItems = orderItems
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            var dto = new OrderSummaryDto(
                order.OrderId,
                order.Status,
                order.PlacedAt,
                order.CompletedAt,
                order.TotalAmount);

            // Return 201 Created so the test expecting CreatedAtActionResult passes
            return CreatedAtAction(nameof(GetById), new { id = order.OrderId }, dto);
        }

        // GET: api/orders/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<object>> GetById(int id)
        {
            var order = await _db.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Combination)
                        .ThenInclude(c => c.MenuItem)
                .SingleOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return NotFound();

            return Ok(new
            {
                order.OrderId,
                order.Status,
                order.PlacedAt,
                order.CompletedAt,
                order.TotalAmount,
                Items = order.OrderItems.Select(oi => new
                {
                    oi.OrderItemId,
                    oi.CombinationId,
                    DrinkName = oi.Combination!.MenuItem!.Name,
                    oi.Quantity,
                    oi.UnitPrice,
                    LineTotal = oi.Quantity * oi.UnitPrice   // compute instead of relying on private setter
                })
            });
        }

        // GET: api/orders?userId=3
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetAll(
            [FromQuery] int? userId = null)
        {
            var query = _db.Orders.AsQueryable();

            if (userId.HasValue)
                query = query.Where(o => o.UserId == userId.Value);

            var list = await query
                .OrderByDescending(o => o.PlacedAt)
                .Select(o => new OrderSummaryDto(
                    o.OrderId,
                    o.Status,
                    o.PlacedAt,
                    o.CompletedAt,
                    o.TotalAmount))
                .ToListAsync();

            return Ok(list);
        }

        // PUT: api/orders/5/status
        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest req)
        {
            var order = await _db.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = req.Status;

            if (string.Equals(req.Status, "Completed", StringComparison.OrdinalIgnoreCase))
                order.CompletedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // GET: api/orders/{orderId}/items
        // Returns item details for a single order (menu item id, name, option names, quantity, unit price, line total)
        [HttpGet("{orderId:int}/items")]
        public async Task<ActionResult<IEnumerable<OrderItemDetailsDto>>> GetOrderItems(int orderId)
        {
            var items = await _db.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .Include(oi => oi.Combination!)
                    .ThenInclude(c => c.MenuItem!)
                .Include(oi => oi.Combination!)
                    .ThenInclude(c => c.CombinationOptions!)
                        .ThenInclude(co => co.OptionValue!)
                .Select(oi => new OrderItemDetailsDto
                {
                    MenuItemId = oi.Combination!.MenuItemId,
                    MenuItemName = oi.Combination.MenuItem!.Name,
                    Options = oi.Combination.CombinationOptions
                                .Select(co => co.OptionValue!.Name)
                                .ToList(),
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    LineTotal = oi.Quantity * oi.UnitPrice   // compute locally
                })
                .ToListAsync();

            if (items == null || items.Count == 0)
                return NotFound();

            return Ok(items);
        }

        // GET: api/orders/popular?n=10
        // Aggregated across last `n` orders: MenuItemId, Name, CategoryId, total quantity, number of orders containing it
        [HttpGet("popular")]
        public async Task<ActionResult<IEnumerable<PopularItemDto>>> GetPopularItems([FromQuery] int n = 10)
        {
            if (n <= 0) return BadRequest("n must be greater than zero");

            // Get last n order ids (ordered by OrderId; change to a Date field if available)
            var recentOrderIds = await _db.Orders
                .OrderByDescending(o => o.OrderId)
                .Take(n)
                .Select(o => o.OrderId)
                .ToListAsync();

            if (recentOrderIds.Count == 0) return Ok(new List<PopularItemDto>());

            var aggregated = await _db.OrderItems
                .Where(oi => recentOrderIds.Contains(oi.OrderId))
                .Include(oi => oi.Combination!)
                    .ThenInclude(c => c.MenuItem!)
                .GroupBy(oi => new
                {
                    MenuItemId = oi.Combination!.MenuItemId,
                    MenuItemName = oi.Combination.MenuItem!.Name,
                    MenuCategoryId = oi.Combination.MenuItem!.MenuCategoryId
                })
                .Select(g => new PopularItemDto
                {
                    MenuItemId = g.Key.MenuItemId,
                    MenuItemName = g.Key.MenuItemName,
                    MenuItemCategoryId = g.Key.MenuCategoryId,
                    TotalQuantity = g.Sum(x => x.Quantity),
                    OrderCount = g.Select(x => x.OrderId).Distinct().Count()
                })
                .OrderByDescending(p => p.TotalQuantity)
                .ToListAsync();

            return Ok(aggregated);
        }
    }
}
