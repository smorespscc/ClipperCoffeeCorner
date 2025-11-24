using System;
using System.Linq;
using System.Threading.Tasks;
using ClipperCoffeeCorner.Data;
using ClipperCoffeeCorner.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClipperCoffeeCorner.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CheckoutController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ISquareCheckoutService _square;

        public CheckoutController(AppDbContext db, ISquareCheckoutService square)
        {
            _db = db;
            _square = square;
        }

        public record PaymentLinkResponse(int OrderId, string PaymentLink);

        // POST: /api/checkout/{orderId}/payment-link
        [HttpPost("{orderId:int}/payment-link")]
        public async Task<ActionResult<PaymentLinkResponse>> CreatePaymentLink(int orderId)
        {
            var order = await _db.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Combination)
                        .ThenInclude(c => c.MenuItem)
                .SingleOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound(new { message = $"Order {orderId} not found." });
            }

            var lineItems = order.OrderItems.Select(oi =>
            {
                var name = oi.Combination?.MenuItem?.Name ?? "Drink";
                var cents = (long)Math.Round(oi.UnitPrice * 100m);

                return new SquareLineItem
                {
                    Name = name,
                    Quantity = oi.Quantity.ToString(),
                    BasePriceAmount = cents,
                    BasePriceCurrency = "USD"
                };
            }).ToList();

            var squareOrder = new SquareOrder
            {
                LineItems = lineItems
                // Taxes can be added here if you want
            };

            var url = await _square.CreatePaymentLinkAsync(squareOrder);

            return Ok(new PaymentLinkResponse(orderId, url));
        }
    }
}
