using System.Diagnostics;
using ClipperCoffeeCorner.Models;
using Microsoft.AspNetCore.Mvc;
using ClipperCoffeeCorner.Dtos.Menu;
using ClipperCoffeeCorner.Dtos.Orders;
using ClipperCoffeeCorner.Dtos.Ui;
using ClipperCoffeeCorner.Dtos.Queue;
using ClipperCoffeeCorner.Dtos.Auth;


namespace ClipperCoffeeCorner.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
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
