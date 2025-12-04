using ClipperCoffeeCorner.Models;
using ClipperCoffeeCorner.Models.Domain;
using ClipperCoffeeCorner.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ClipperCoffeeCorner.Controllers
{
    /// <summary>
    /// API Controller for menu operations.
    /// Handles menu items, service periods, and availability.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MenuController : ControllerBase
    {
        private readonly IMenuService _menuService;
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly ILogger<MenuController> _logger;

        public MenuController(
            IMenuService menuService,
            IOrderService orderService,
            ICustomerService customerService,
            ILogger<MenuController> logger)
        {
            _menuService = menuService ?? throw new ArgumentNullException(nameof(menuService));
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ==================== MENU ITEMS ====================

        /// <summary>
        /// Gets all menu items
        /// GET /api/menu/items
        /// </summary>
        [HttpGet("items")]
        public async Task<ActionResult> GetAllMenuItems()
        {
            try
            {
                var items = await _menuService.GetAllMenuItemsAsync();
                return Ok(new { items });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting menu items");
                return StatusCode(500, new { message = "An error occurred while retrieving menu items" });
            }
        }

        /// <summary>
        /// Gets available menu items for current service period
        /// GET /api/menu/available
        /// </summary>
        [HttpGet("available")]
        public async Task<ActionResult> GetAvailableMenuItems()
        {
            try
            {
                var items = await _menuService.GetAvailableMenuItemsAsync();
                return Ok(new { items });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available menu items");
                return StatusCode(500, new { message = "An error occurred while retrieving available items" });
            }
        }

        /// <summary>
        /// Gets trending menu items
        /// GET /api/menu/trending
        /// </summary>
        [HttpGet("trending")]
        public async Task<ActionResult> GetTrendingItems()
        {
            try
            {
                var items = await _menuService.GetTrendingItemsAsync();
                return Ok(new { items });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting trending items");
                return StatusCode(500, new { message = "An error occurred while retrieving trending items" });
            }
        }

        /// <summary>
        /// Gets special menu items
        /// GET /api/menu/specials
        /// </summary>
        [HttpGet("specials")]
        public async Task<ActionResult> GetSpecialItems()
        {
            try
            {
                var items = await _menuService.GetSpecialItemsAsync();
                return Ok(new { items });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting special items");
                return StatusCode(500, new { message = "An error occurred while retrieving special items" });
            }
        }

        // ==================== SERVICE PERIOD ====================

        /// <summary>
        /// Gets current service period
        /// GET /api/menu/service-period
        /// </summary>
        [HttpGet("service-period")]
        public async Task<ActionResult> GetServicePeriod()
        {
            try
            {
                var period = await _menuService.GetCurrentServicePeriodAsync();
                var hours = await _menuService.GetServiceHoursAsync(period);
                var isOpen = await _menuService.IsCurrentlyOpenAsync();

                return Ok(new
                {
                    period = period.ToString(),
                    isOpen,
                    startTime = hours.start.ToString(@"hh\:mm"),
                    endTime = hours.end.ToString(@"hh\:mm")
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service period");
                return StatusCode(500, new { message = "An error occurred while retrieving service period" });
            }
        }

        /// <summary>
        /// Checks if cafe is currently open
        /// GET /api/menu/is-open
        /// </summary>
        [HttpGet("is-open")]
        public async Task<ActionResult> IsOpen()
        {
            try
            {
                var isOpen = await _menuService.IsCurrentlyOpenAsync();
                return Ok(new { isOpen });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if open");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        // ==================== PRICING ====================

        /// <summary>
        /// Gets base price for an item
        /// GET /api/menu/price/{itemName}
        /// </summary>
        [HttpGet("price/{itemName}")]
        public async Task<ActionResult> GetItemPrice(string itemName)
        {
            try
            {
                var price = await _menuService.GetBasePriceAsync(itemName);
                return Ok(new { itemName, price });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting item price");
                return StatusCode(500, new { message = "An error occurred while retrieving price" });
            }
        }

        /// <summary>
        /// Gets available modifiers for item type
        /// GET /api/menu/modifiers/{type}
        /// </summary>
        [HttpGet("modifiers/{type}")]
        public async Task<ActionResult> GetModifiers(string type)
        {
            try
            {
                var itemType = type.ToLower() == "drink" ? MenuItemType.Drink : MenuItemType.Food;
                var modifiers = await _menuService.GetAvailableModifiersAsync(itemType);
                return Ok(new { modifiers });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting modifiers");
                return StatusCode(500, new { message = "An error occurred while retrieving modifiers" });
            }
        }
    }
}
