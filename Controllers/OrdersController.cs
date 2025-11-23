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

        // =========================
        // DTOs
        // =========================

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

        // For "items in one order" (notification details)
        public record OrderItemNotificationDto(
            int MenuItemId,
            string MenuItemName,
            List<string> OptionNames,
            int Quantity,
            decimal UnitPrice,
            decimal LineTotal);

        // For "popular items in last n orders"
        public record PopularMenuItemDto(
            int MenuItemId,
            int MenuItemCategoryId,
            string MenuItemName,
            int TotalQuantity,
            int OrdersCount);

        // =========================
        // 1. Create Order
        // POST: api/orders
        // =========================
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
                    // LineTotal is computed in SQL
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

        // =========================
        // 2. Get single order (full)
        // GET: api/orders/5
        // =========================
        [HttpGet("{id:int}")]
        public async Task<ActionResult<object>> GetById(int id)
        {
            var order = await _db.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Combination)
                        .ThenInclude(c => c.MenuItem)
                .SingleOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return NotFound();

            var itemsDto = order.OrderItems.Select(oi =>
            {
                var combo = oi.Combination;
                var menuItem = combo?.MenuItem;

                var drinkName = menuItem?.Name ?? "Unknown";
                var lineTotal = oi.LineTotal != 0m
                    ? oi.LineTotal
                    : oi.UnitPrice * oi.Quantity;

                return new
                {
                    oi.OrderItemId,
                    oi.CombinationId,
                    DrinkName = drinkName,
                    oi.Quantity,
                    oi.UnitPrice,
                    LineTotal = lineTotal
                };
            });

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

        // =========================
        // 3. Get many orders (summary)
        // GET: api/orders?userId=3
        // =========================
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

        // =========================
        // 4. Update order status
        // PUT: api/orders/5/status
        // =========================
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
