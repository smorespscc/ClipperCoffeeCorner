using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using ClipperCoffeeCorner.Models;
using ClipperCoffeeCorner.Extensions;
using ClipperCoffeeCorner.Data;
using ClipperCoffeeCorner.Services;
using System;
using System.Linq;

namespace Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ISquareCheckoutService _squareService;
        private readonly AppDbContext _dbContext;

        public CheckoutController(ISquareCheckoutService squareService, AppDbContext dbContext)
        {
            _squareService = squareService;
            _dbContext = dbContext;
        }

        // GET: /Checkout
        public IActionResult Index()
        {
            // Example view where you show product and a "Checkout" button
            return View();
        }

        // POST: /Checkout/CreateLink
        [HttpGet]
        public async Task<IActionResult> CreateLink()
        {
            // For demo: create a $10.00 item (1000 cents) if no order exists in session
            var redirect = Url.Action("Success", "Checkout", null, Request.Scheme);

            // Attempt to read an existing order from session
            var order = HttpContext.Session.GetObject<Order>("CurrentOrder");
            if (order == null)
            {
                // Compose order object from the database where OrderId == 1 for demo purposes
                order = await _dbContext.Set<Order>()
                .Include(o => o.OrderItems)
                                        .Include(o => o.User)
                                        .FirstOrDefaultAsync(o => o.OrderId == 1);
            if (order == null)
            {
                    return BadRequest("No order found in database with id = 1.");
            }

                // Ensure required fields have sensible defaults for the checkout flow
                if (order.IdempotencyKey == Guid.Empty)
            {
                    order.IdempotencyKey = Guid.NewGuid();
                }

                if (string.IsNullOrWhiteSpace(order.Status))
                {
                    order.Status = "Pending";
                }

                if (order.PlacedAt == default)
            {
                    order.PlacedAt = DateTime.UtcNow;
                }

                // Ensure OrderItems collection is not null (for services that iterate)
                order.OrderItems ??= Enumerable.Empty<OrderItem>().ToList();
            }

            // Persist updated order back into session so later steps can access it
            HttpContext.Session.SetObject("CurrentOrder", order);

            // Create payment link from Square using the order stored in session
            // (Service expects Order and optional redirect URL)
            var linkUrl = await _squareService.CreatePaymentLinkAsync(order, redirect);

            // Redirect user to Square hosted checkout
            return Redirect(linkUrl);
        }

        public IActionResult Success()
        {
            // remove order from session
            HttpContext.Session.Remove("CurrentOrder");
            // redirect to home index
            return RedirectToAction("Index", "Home");
        }
    }
}
