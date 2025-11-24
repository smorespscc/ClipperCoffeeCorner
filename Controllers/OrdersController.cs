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
                order.UserId, // !!!
                order.Status,
                order.PlacedAt,
                order.CompletedAt,
                order.TotalAmount,
                Items = itemsDto
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

        // ============================================================
        // 5. NOTIFICATION ENDPOINT #1
        //    Items in a specific order (with option names)
        //    GET: api/orders/{orderId}/items-detail
        // ============================================================
        [HttpGet("{orderId:int}/items-detail")]
        public async Task<ActionResult<IEnumerable<OrderItemNotificationDto>>> GetOrderItemsDetail(
            int orderId)
        {
            var orderItems = await _db.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .Include(oi => oi.Combination)
                    .ThenInclude(c => c.MenuItem)
                .Include(oi => oi.Combination)
                    .ThenInclude(c => c.CombinationOptions)
                        .ThenInclude(co => co.OptionValue)
                .ToListAsync();

            if (!orderItems.Any())
            {
                return NotFound(new { message = $"No items found for order {orderId}." });
            }

            var list = orderItems
                .Where(oi => oi.Combination != null && oi.Combination.MenuItem != null)
                .Select(oi =>
                {
                    var combo = oi.Combination!;
                    var menuItem = combo.MenuItem!;

                    var optionNames = combo.CombinationOptions?
                        .Where(co => co.OptionValue != null)
                        .Select(co => co.OptionValue!.Name)
                        .ToList()
                        ?? new List<string>();

                    var lineTotal = oi.LineTotal != 0m
                        ? oi.LineTotal
                        : oi.UnitPrice * oi.Quantity;

                    return new OrderItemNotificationDto(
                        MenuItemId: combo.MenuItemId,
                        MenuItemName: menuItem.Name,
                        OptionNames: optionNames,
                        Quantity: oi.Quantity,
                        UnitPrice: oi.UnitPrice,
                        LineTotal: lineTotal);
                })
                .ToList();

            return Ok(list);
        }

        // ============================================================
        // 6. NOTIFICATION ENDPOINT #2
        //    Popular items in last n orders (aggregated)
        //    GET: api/orders/popular-items?n=10
        // ============================================================
        [HttpGet("popular-items")]
        public async Task<ActionResult<IEnumerable<PopularMenuItemDto>>> GetPopularMenuItems(
            [FromQuery] int n = 10)
        {
            if (n <= 0) n = 10;

            // 1) Get the IDs of the last n orders
            var lastOrderIds = await _db.Orders
                .OrderByDescending(o => o.PlacedAt)
                .Take(n)
                .Select(o => o.OrderId)
                .ToListAsync();

            if (lastOrderIds.Count == 0)
            {
                return Ok(Array.Empty<PopularMenuItemDto>());
            }

            // 2) Load items from those orders, with their MenuItem info
            var items = await _db.OrderItems
                .Where(oi => lastOrderIds.Contains(oi.OrderId))
                .Include(oi => oi.Combination)
                    .ThenInclude(c => c.MenuItem)
                .Where(oi => oi.Combination != null && oi.Combination.MenuItem != null)
                .Select(oi => new
                {
                    oi.OrderId,
                    oi.Quantity,
                    MenuItemId = oi.Combination!.MenuItemId,
                    MenuItemCategoryId = oi.Combination!.MenuItem!.MenuCategoryId,
                    MenuItemName = oi.Combination!.MenuItem!.Name
                })
                .ToListAsync();

            if (items.Count == 0)
            {
                return Ok(Array.Empty<PopularMenuItemDto>());
            }

            // 3) Group in memory and aggregate
            var result = items
                .GroupBy(x => new { x.MenuItemId, x.MenuItemCategoryId, x.MenuItemName })
                .Select(g => new PopularMenuItemDto(
                    MenuItemId: g.Key.MenuItemId,
                    MenuItemCategoryId: g.Key.MenuItemCategoryId,
                    MenuItemName: g.Key.MenuItemName,
                    TotalQuantity: g.Sum(x => x.Quantity),
                    OrdersCount: g.Select(x => x.OrderId).Distinct().Count()))
                .OrderByDescending(x => x.TotalQuantity)
                .ToList();

            return Ok(result);
        }
    }
}
