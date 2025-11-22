using System;
using System.Threading.Tasks;
using ClipperCoffeeCorner.Data;
using ClipperCoffeeCorner.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClipperCoffeeCorner.Controllers
{
    public record CreatePaymentRequest(
        int OrderId,
        decimal? Amount,
        string? RedirectUrl
    );

    public record PaymentDto(
        int PaymentId,
        int OrderId,
        decimal Amount,
        string Provider,
        string Status,
        string? CheckoutUrl
    );

    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public PaymentsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<ActionResult<PaymentDto>> Create([FromBody] CreatePaymentRequest req)
        {
            var order = await _db.Orders.SingleOrDefaultAsync(o => o.OrderId == req.OrderId);
            if (order == null)
            {
                return NotFound($"Order {req.OrderId} not found.");
            }

            var amount = req.Amount ?? order.TotalAmount;

            var idempotencyKey = Guid.NewGuid();

            var payment = new Payment
            {
                OrderId = order.OrderId,
                Amount = amount,
                Provider = "Square",
                Status = "Pending",
                IdempotencyKey = idempotencyKey,
                CreatedAt = DateTime.UtcNow
            };

            _db.Payments.Add(payment);
            await _db.SaveChangesAsync();

            var dto = new PaymentDto(
                payment.PaymentId,
                payment.OrderId,
                payment.Amount,
                payment.Provider,
                payment.Status,
                payment.CheckoutUrl
            );

            return CreatedAtAction(
                nameof(GetById),
                new { id = payment.PaymentId },
                dto);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<PaymentDto>> GetById(int id)
        {
            var payment = await _db.Payments
                .AsNoTracking()
                .SingleOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null) return NotFound();

            var dto = new PaymentDto(
                payment.PaymentId,
                payment.OrderId,
                payment.Amount,
                payment.Provider,
                payment.Status,
                payment.CheckoutUrl
            );

            return Ok(dto);
        }
    }
}
