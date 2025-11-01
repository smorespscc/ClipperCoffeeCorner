using Microsoft.AspNetCore.Mvc;

namespace ClipperCoffeeCorner.Controllers
{
    [ApiController]
    [Route("menu")]
    public class MenuController : ControllerBase
    {
        // GET /menu
        [HttpGet]
        public IActionResult GetMenu()
        {
            // matches the UI doc (HOT / COLD / FOOD + modifiers)
            var result = new
            {
                currentServiceWindow = new { label = "Breakfast!", start = "08:00", end = "10:30" },
                categories = new object[]
                {
                    new {
                        id = "hot",
                        name = "HOT",
                        items = new object[]
                        {
                            new { id = "espresso",      name = "Espresso",      price = 3.00, isAvailable = true },
                            new { id = "matcha-latte",  name = "Matcha Latte",  price = 4.75, isAvailable = true },
                            new { id = "cappuccino",    name = "Cappuccino",    price = 4.25, isAvailable = true },
                            new { id = "mocha",         name = "Mocha",         price = 4.95, isAvailable = true },
                            new { id = "hot-chocolate", name = "Hot Chocolate", price = 3.95, isAvailable = true }
                        }
                    },
                    new {
                        id = "cold",
                        name = "COLD",
                        items = new object[]
                        {
                            new { id = "lotus",         name = "Lotus",         price = 4.50, isAvailable = true },
                            new { id = "refresher",     name = "Refresher",     price = 4.25, isAvailable = true },
                            new { id = "italian-soda",  name = "Italian Soda",  price = 3.75, isAvailable = true },
                            new { id = "lemonade",      name = "Lemonade",      price = 3.50, isAvailable = true }
                        }
                    },
                    new {
                        id = "food",
                        name = "FOOD",
                        items = new object[]
                        {
                            new { id = "chicken-strips", name = "Chicken Str.",  price = 5.50, isAvailable = true },
                            new { id = "blt",            name = "BLT",           price = 5.25, isAvailable = true },
                            new { id = "potato-salad",   name = "Potato Salad",  price = 3.25, isAvailable = true },
                            new { id = "soup",           name = "Soup",          price = 4.25, isAvailable = true }
                        }
                    }
                },
                drinkModifiers = new
                {
                    style = new[] {
                        new { id = "hot",  label = "Hot" },
                        new { id = "iced", label = "Iced" }
                    },
                    milk = new[] {
                        new { id = "dairy", label = "Dairy" },
                        new { id = "oat",   label = "Oat" }
                    },
                    flavors = new[] {
                        new { id = "vanilla",  label = "Vanilla" },
                        new { id = "hazelnut", label = "Hazelnut" },
                        new { id = "mint",     label = "Mint" },
                        new { id = "cinnamon", label = "Cinnamon" }
                    },
                    additions = new[] {
                        new { id = "whip",       label = "Whip Cream" },
                        new { id = "extra-shot", label = "Extra Shot" }
                    },
                    restrictions = new[] {
                        new { id = "sugar-free",    label = "Sugar Free" },
                        new { id = "caffeine-free", label = "Caffeine Free" }
                    }
                },
                foodModifiers = new
                {
                    sauces = new[] {
                        new { id = "ranch",         label = "Ranch" },
                        new { id = "honey-mustard", label = "Honey Mustard" },
                        new { id = "bbq",           label = "BBQ" }
                    }
                }
            };

            return Ok(result);
        }
    }
}
