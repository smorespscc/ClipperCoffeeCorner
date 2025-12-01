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
            return RedirectToAction("Queue", "Home");
        }
    }
}
