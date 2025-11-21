using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClipperCoffeeCorner.Data;

namespace ClipperCoffeeCorner.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly AppDbContext _db;

        public MenuController(AppDbContext db)
        {
            _db = db;
        }

        // GET: api/menu/categories
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _db.MenuCategories
                .OrderBy(c => c.MenuCategoryId)
                .Select(c => new
                {
                    c.MenuCategoryId,
                    c.ParentCategoryId,
                    c.Name
                })
                .ToListAsync();

            return Ok(categories);
        }

        // GET: api/menu/items
        // GET: api/menu/items?categoryId=2
        //
        // If categoryId is a parent (like Hot = 2),
        // this will return items in that category PLUS
        // all direct children (Coffee = 7, Tea = 8, etc.).
        [HttpGet("items")]
        public async Task<IActionResult> GetItems([FromQuery] int? categoryId)
        {
            // base query: only active items, and include category so we can show the name
            var query = _db.MenuItems
                .Include(m => m.MenuCategory)
                .Where(m => m.IsActive);

            if (categoryId.HasValue)
            {
                var catId = categoryId.Value;

                // IDs to include: the category itself + any children of it
                var categoryIds = await _db.MenuCategories
                    .Where(c => c.MenuCategoryId == catId || c.ParentCategoryId == catId)
                    .Select(c => c.MenuCategoryId)
                    .ToListAsync();

                query = query.Where(m => categoryIds.Contains(m.MenuCategoryId));
            }

            var items = await query
                .OrderBy(m => m.MenuCategoryId)
                .ThenBy(m => m.Name)
                .Select(m => new
                {
                    m.MenuItemId,
                    m.MenuCategoryId,
                    CategoryName = m.MenuCategory!.Name,
                    m.Name,
                    m.BasePrice,
                    m.Description,
                    m.IsActive
                })
                .ToListAsync();

            return Ok(items);
        }

        // GET: api/menu/combinations?menuItemId=1
        [HttpGet("combinations")]
        public async Task<IActionResult> GetCombinations([FromQuery] int menuItemId)
        {
            var combos = await _db.Combinations
                .Where(c => c.MenuItemId == menuItemId && c.IsActive)
                .Select(c => new
                {
                    c.CombinationId,
                    c.MenuItemId,
                    c.Code,
                    c.Price,
                    c.IsActive
                })
                .ToListAsync();

            return Ok(combos);
        }
    }
}