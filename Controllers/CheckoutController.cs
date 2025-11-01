using Microsoft.AspNetCore.Mvc;
using Services;
using System.Threading.Tasks;

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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLink()
        {
            // For demo: create a $10.00 item (1000 cents)
            var itemName = "Coffee Product";
            long amountCents = 1000;
            var redirect = Url.Action("Success", "Checkout", null, Request.Scheme);

            var linkUrl = await _squareService.CreatePaymentLinkAsync(itemName, amountCents, "USD", redirect);

            // Redirect user to Square hosted checkout
            return Redirect(linkUrl);
        }

        public IActionResult Success()
        {
            // Square will redirect here after checkout (if configured)
            return View();
        }
    }
}