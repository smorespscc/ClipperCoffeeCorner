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
                    oi.LineTotal   // ✅ read-only is fine
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
    }
}
