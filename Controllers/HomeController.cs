using System.Diagnostics;
using System.Threading.Tasks;
using ClipperCoffeeCorner.Models;
using ClipperCoffeeCorner.Models.ViewModels;
using ClipperCoffeeCorner.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClipperCoffeeCorner.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMenuService _menuService;
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IQueueService _queueService;
        private readonly IPricingService _pricingService;

        public HomeController(
            ILogger<HomeController> logger,
            IMenuService menuService,
            IOrderService orderService,
            ICustomerService customerService,
            IQueueService queueService,
            IPricingService pricingService)
        {
            _logger = logger;
            _menuService = menuService;
            _orderService = orderService;
            _customerService = customerService;
            _queueService = queueService;
            _pricingService = pricingService;
        }

        public IActionResult Index()
        {
            return View(new LoginViewModel());
        }

        public async Task<IActionResult> Menu()
        {
            var sessionId = HttpContext.Session.Id;
            var customerId = await _customerService.GetCurrentCustomerIdAsync(sessionId);
            var isStaff = await _customerService.IsStaffSessionAsync(sessionId);
            
            var viewModel = new MenuViewModel
            {
                CurrentServicePeriod = await _menuService.GetCurrentServicePeriodAsync(),
                AllMenuItems = await _menuService.GetAllMenuItemsAsync(),
                DrinkItems = await _menuService.GetMenuItemsByTypeAsync(MenuItemType.Drink),
                FoodItems = await _menuService.GetMenuItemsByTypeAsync(MenuItemType.Food),
                TrendingItems = await _menuService.GetTrendingItemsAsync(),
                SpecialItems = await _menuService.GetSpecialItemsAsync(),
                IsAuthenticated = !string.IsNullOrEmpty(customerId),
                IsStaff = isStaff,
                CustomerId = customerId
            };

            var hours = await _menuService.GetServiceHoursAsync(viewModel.CurrentServicePeriod);
            viewModel.ServiceHours = $"{hours.start:hh\\:mm} - {hours.end:hh\\:mm}";

            if (!string.IsNullOrEmpty(customerId))
            {
                viewModel.SavedOrders = await _orderService.GetSavedOrdersAsync(customerId);
                viewModel.RecentOrders = await _orderService.GetRecentOrdersAsync(customerId);
            }

            var cart = await _orderService.GetCartAsync(sessionId);
            viewModel.CartItems = cart.Items;
            viewModel.CartSubtotal = cart.Subtotal;

            return View(viewModel);
        }

        public async Task<IActionResult> Checkout()
        {
            var sessionId = HttpContext.Session.Id;
            var isStaff = await _customerService.IsStaffSessionAsync(sessionId);
            var cart = await _orderService.GetCartAsync(sessionId);

            var viewModel = new CheckoutViewModel
            {
                CartItems = cart.Items,
                Subtotal = cart.Subtotal,
                StaffDiscount = cart.StaffDiscount,
                Total = cart.Total,
                IsStaff = isStaff
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Payment()
        {
            var sessionId = HttpContext.Session.Id;
            var isStaff = await _customerService.IsStaffSessionAsync(sessionId);
            var cart = await _orderService.GetCartAsync(sessionId);

            var viewModel = new PaymentViewModel
            {
                OrderItems = cart.Items,
                Subtotal = cart.Subtotal,
                StaffDiscount = cart.StaffDiscount,
                Total = cart.Total,
                IsStaff = isStaff
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Queue()
        {
            var queue = await _queueService.GeneratePlaceholderQueueAsync();
            
            var viewModel = new QueueViewModel
            {
                QueueEntries = queue
            };

            return View(viewModel);
        }

        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        public IActionResult Closed()
        {
            return View();
        }

        public IActionResult Contributions()
        {
            return View();
        }

        public IActionResult MenuPreview()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
