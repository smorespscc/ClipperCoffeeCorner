using Microsoft.AspNetCore.Mvc;
using Services;
using System.Threading.Tasks;
using ClipperCoffeeCorner.Models;
using ClipperCoffeeCorner.Extensions;
using System;
using System.Collections.Generic;
using System.Configuration;

namespace Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ISquareCheckoutService _squareService;

        public CheckoutController(ISquareCheckoutService squareService)
        {
            _squareService = squareService;
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
                // minimal order skeleton — fill as appropriate for your Square integration
                order = new Order
                {
                    IdempotencyKey = Guid.NewGuid().ToString("N"),
                    CreatedAt = DateTimeOffset.UtcNow,
                    LineItems = new List<LineItem>
                    {
                        new LineItem
                        {
                            Name = "Coffee",
                            BasePriceMoney = new Money
                            {
                                Amount = 450,
                                Currency = "USD"
                            },
                            Quantity = "2"
                        },
                        new LineItem
                        {
                            Name = "Buttered Croissant",
                            BasePriceMoney = new Money
                            {
                                Amount = 600,
                                Currency = "USD"
                            },
                            Quantity = "1"
                        }
                    },
                    Taxes = new List<TaxLine>
                    {
                        new TaxLine
                        {
                            Name = "Sales Tax",
                            Percentage = "10.1",
                            Scope = "ORDER"
                        },
                    },
                    TotalMoney = 1600,
                    Status = OrderStatus.Draft
                };
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