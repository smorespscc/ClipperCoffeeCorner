using ClipperCoffeeCorner.Data;
using ClipperCoffeeCorner.Models;
using Microsoft.EntityFrameworkCore;

// This will probably all be replaced with calls to stuff the DB team makes so it's placeholder stuff for now

namespace ClipperCoffeeCorner.Services
{
    public class DbOrderRepository : IOrderRepository
    {
        private readonly AppDbContext _db;
        private readonly ILogger<DbOrderRepository> _logger;

        public DbOrderRepository(AppDbContext db, ILogger<DbOrderRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        // add order to database
        public void Add(Order order)
        {
            // replace with whatever DB team makes
            _db.Orders.Add(order);
            _db.SaveChanges();
        }

        // mark order as completed and update columns
        public void CompleteOrder(Guid uid)
        {
            // INSERT: Retrieve the order from the database
            // hardcoded order for testing
            var order = CreateTestOrder();

            // update columns
            order.CompletedAt = DateTimeOffset.Now;
            order.Status = OrderStatus.Completed;

            // save changes to database
            _db.SaveChanges();
        }


        // Create test object
        // delete later
        public static Order CreateTestOrder()
        {
            return new Order
            {
                OrderId = 1001,
                IdempotencyKey = "idemp-12345-abcde-67890",
                CustomerId = "cust_789",
                Currency = "USD",

                // Line Items: Two coffee drinks
                LineItems = new List<LineItem>
        {
            new LineItem
            {
                CatalogObjectId = "CAT_LATTE_001",
                Name = "Latte",
                BasePriceMoney = new Money { Amount = 450, Currency = "USD" }, // $4.50
                Quantity = "1"
            },
            new LineItem
            {
                CatalogObjectId = "CAT_CROISSANT_002",
                Name = "Butter Croissant",
                BasePriceMoney = new Money { Amount = 375, Currency = "USD" }, // $3.75
                Quantity = "2"
            }
        },

                // Taxes: 8.25% sales tax applied at order level
                Taxes = new List<TaxLine>
        {
            new TaxLine
            {
                OrderId = "1001",
                Type = "ADDITIVE",
                Name = "CA Sales Tax",
                Percentage = "8.25",
                Scope = "ORDER"
            }
        },

                // Discounts: None for now (or add one if needed)
                Discounts = new List<DiscountLine>(),

                // Service Charges: e.g., auto-gratuity or delivery fee
                ServiceCharges = new List<ServiceCharge>
        {
            new ServiceCharge
            {
                OrderId = "1001",
                Name = "Counter Service",
                Percentage = "0",
                Taxable = false
            }
        },

                // Computed totals (in cents)
                SubtotalMoney = 450 + (375 * 2),         // $4.50 + $7.50 = $12.00 → 1200 cents
                TotalTaxMoney = 99,                      // ~8.25% of $12.00 ≈ $0.99
                TotalDiscountMoney = 0,
                TotalMoney = 1200 + 99,                  // $12.99

                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-10),
                PlacedAt = DateTimeOffset.UtcNow.AddMinutes(-5),
                CompletedAt = null, // not ready yet

                Alterations = new List<OrderAlteration>
        {
            new OrderAlteration
            {
                Timestamp = DateTimeOffset.UtcNow.AddMinutes(-8),
                Description = "Order created via mobile app",
                ChangedBy = "mobile-app-v2"
            }
        },

                Status = OrderStatus.Placed
            };
        }
    }
}
