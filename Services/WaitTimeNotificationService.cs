using ClipperCoffeeCorner.Models;


namespace ClipperCoffeeCorner.Services
{
    public class WaitTimeNotificationService
    {
        private readonly IWaitTimeEstimator _estimator;
        private readonly IEnumerable<INotificationService> _notifier;
        private readonly ILogger<WaitTimeNotificationService> _logger;
        private readonly IOrderRepository _orders;

        public WaitTimeNotificationService(
            IOrderRepository orders,
            IWaitTimeEstimator estimator,
            IEnumerable<INotificationService> notifier,
            ILogger<WaitTimeNotificationService> logger)
        {
            _orders = orders;
            _estimator = estimator;
            _notifier = notifier;
            _logger = logger;
        }

        public async Task<double> AddOrderAsync(Order order)
        {
            ArgumentNullException.ThrowIfNull(order);

            // set order fields
            order.PlacedAt = DateTimeOffset.Now;
            order.Status = OrderStatus.Placed;

            // ask ML estimation service for wait time (can change this depending on what ML service actually needs)
            double estimatedWaitTime = _estimator.Estimate(order);

            // fill in fields from ML estimation stuff

            // add to orders table
            _orders.Add(order);

            // INSERT: stuff to query users table to get email, phone number, and notification preference
            // both SMS and Email notifiers will need that info so better to query it here than have them both do it
            User user = new User
            {
                // temporary hardcoded user info for testing
                UserId = Guid.NewGuid(),
                Username = "Test User",
                UserRole = "Customer",
                NotificationPref = NotificationPreference.Email,
                PhoneNumber = "+15551234567",
                NotificationEmail = "mmarsh7of9@gmail.com"
            };

            // send confirmation SMS or Email
            foreach (var notifier in _notifier)
            {
                await notifier.SendPlacedAsync(order, user, estimatedWaitTime);
            }
            return estimatedWaitTime;
        }

        public async Task<Order> CompleteOrderAsync(Guid orderId)
        {
            // INSERT: retrieve order from sql table
            // placeholder object for now
            var order = CreateTestOrder();

            // complete order by updating table
            _orders.CompleteOrder(orderId);

            // INSERT: stuff to query users table to get email, phone number, and notification preference
            // both SMS and Email notifiers will need that info so better to query it here than have them both do it
            User user = new User
            {
                // temporary hardcoded user info for testing
                UserId = Guid.NewGuid(),
                Username = "Test User",
                UserRole = "Customer",
                NotificationPref = NotificationPreference.Email,
                PhoneNumber = "+15551234567",
                NotificationEmail = "mmarsh7of9@gmail.com"
            };

            // Send ready SMS or Email
            foreach (var notifier in _notifier)
            {
                await notifier.SendCompletionAsync(order, user);
            }

            // give to ML training service. Might not need this.
            _estimator.AddCompletedForTraining(order);

            return order;
        }


        // subject to changes based on DB team service
        public async Task<List<PopularItemsModel>> GetPopularItemsAsync(Guid menuCategory)
        {
            // INSERT: call to DB to get popular items from orders table
            // maybe just get a view of last 100 orders or something idk

            await Task.Delay(100); // simulate async DB call

            // placeholder hardcoded list for now
            return new List<PopularItemsModel>
            {
                new PopularItemsModel
                {
                    MenuItemId = Guid.NewGuid(),
                    Name = "Cappuccino",
                    OrderCount = 150
                },
                new PopularItemsModel
                {
                    MenuItemId = Guid.NewGuid(),
                    Name = "Espresso",
                    OrderCount = 120
                },
                new PopularItemsModel
                {
                    MenuItemId = Guid.NewGuid(),
                    Name = "Blueberry Muffin",
                    OrderCount = 90
                }
            };
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