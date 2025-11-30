using Microsoft.AspNetCore.Mvc;

namespace ClipperCoffeeCorner.Controllers
{
    public class QueueController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
