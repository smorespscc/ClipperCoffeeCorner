using ClipperCoffeeCorner.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;


namespace ClipperCoffeeCorner.Services
{
    public class WaitTimeNotificationService
    {
        private readonly string? _testToEmail;
        private readonly string? _testToPhoneNumber;
        private readonly IWaitTimeEstimator _estimator;
        private readonly IEnumerable<INotificationService> _notifier;
        private readonly ILogger<WaitTimeNotificationService> _logger;
        private readonly HttpClient _http;

        public WaitTimeNotificationService(
            IConfiguration config,
            IWaitTimeEstimator estimator,
            IEnumerable<INotificationService> notifier,
            ILogger<WaitTimeNotificationService> logger,
            IHttpClientFactory factory)
        {
            _testToEmail = config["NotificationTestDetails:ToEmail"]!;
            _testToPhoneNumber = config["NotificationTestDetails:ToPhoneNumber"]!;
            _estimator = estimator;
            _notifier = notifier;
            _logger = logger;
            _http = factory.CreateClient();
            _http.BaseAddress = new Uri("https://localhost:7138"); // insert app URL when deployed
        }

        // =========================
        // === Process New Order ===
        // =========================
        // 1. get estimated wait time
        // 2. send notification
        // 3. return estimated wait time if UI wants to display it in the app or something
        public async Task<double> ProcessNewOrder(int orderId)
        {
            ArgumentNullException.ThrowIfNull(orderId);

            // get order from Orders API
            var order = await GetOrderById(orderId);

            // get order item details from Orders API
            var itemsResponse = await _http.GetAsync($"/api/orders/{order.OrderId}/items-detail");

            if (!itemsResponse.IsSuccessStatusCode)
            {
                var error = await itemsResponse.Content.ReadAsStringAsync();
                throw new Exception($"Failed to fetch order items: {error}");
            }

            var detailedItems =
                await itemsResponse.Content.ReadFromJsonAsync<List<OrderItemDetailsDto>>()
                ?? new List<OrderItemDetailsDto>();

            // get estimated wait time
            double estimatedWaitTime = _estimator.Estimate(order, detailedItems);

            // I think the order is getting added to the DB before this gets called,
            // but if not, then need to add a call to CreateOrder in OrdersController here

            // only send notification if order is made by a registered user
            // to make it work for guest orders, probably add email/phone number to the Order model
            if (order.UserId.HasValue)
            {
                // query users table to get email, phone number, and notification preference
                var userResponse = await _http.GetAsync($"/api/users/{order.UserId}");
                userResponse.EnsureSuccessStatusCode();
                var user = await userResponse.Content.ReadFromJsonAsync<UserResponse>();

                // send confirmation SMS or Email
                foreach (var notifier in _notifier)
                {
                    #pragma warning disable CS8604 // Possible null reference argument.
                    await notifier.SendPlacedAsync(order, user, estimatedWaitTime, detailedItems);
                    #pragma warning restore CS8604 // Possible null reference argument.
                }
            }

            return estimatedWaitTime;
        }

        // ===============================
        // === Complete Existing Order ===
        // ===============================
        // 1. send completion notification to customer
        // 2. give order to ML training service (might not be necessary)
        public async Task CompleteOrderAsync(int orderId)
        {
            // get order
            var orderResponse = await _http.GetAsync($"/api/orders/{orderId}");
            orderResponse.EnsureSuccessStatusCode();
            var order = await orderResponse.Content.ReadFromJsonAsync<OrderDetailsDto>();

            // get order details
            var itemsResponse = await _http.GetAsync($"/api/orders/{orderId}/items-detail");

            if (!itemsResponse.IsSuccessStatusCode)
            {
                var error = await itemsResponse.Content.ReadAsStringAsync();
                throw new Exception($"Failed to fetch order items: {error}");
            }
            var detailedItems =
                await itemsResponse.Content.ReadFromJsonAsync<List<OrderItemDetailsDto>>()
                ?? new List<OrderItemDetailsDto>();

            if (order is not null && order.UserId.HasValue)
            {
                // query users table to get email, phone number, and notification preference
                var userResponse = await _http.GetAsync($"/api/users/{order.UserId}");
                userResponse.EnsureSuccessStatusCode();
                var user = await userResponse.Content.ReadFromJsonAsync<UserResponse>()
                    ?? throw new InvalidOperationException($"User {order.UserId.Value} not found but was referenced by order {orderId}");

                // send confirmation SMS or Email
                foreach (var notifier in _notifier)
                {
                    await notifier.SendCompletionAsync(order, user, detailedItems);
                }
            }

            // give to ML training service. Might not need this.
            if (order is not null)
            {
                _estimator.AddCompletedForTraining(order, detailedItems);
            }
        }

        // =========================
        // === Get Popular Items ===
        // =========================
        // 1. call Orders API to get popular items
        // 2. sort by MenuCategory if provided
        // 3. return most popular n items (or top 10 by default)
        public async Task<List<PopularItemDto>> GetPopularItemsAsync(int? menuCategoryId)
        {
            var response =
                await _http.GetAsync($"/api/orders/popular-items?n={50}"); // gets popular items based on 50 most recent orders

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to fetch popular items: {error}");
            }

            var items =
                await response.Content.ReadFromJsonAsync<List<PopularItemDto>>();

            // Filter by category if requested
            if (menuCategoryId.HasValue)
            {
                items = items
                    .Where(i => i.MenuItemCategoryId == menuCategoryId.Value)
                    .ToList();
            }

            // Order by popularity again just to be safe
            items = items
                .OrderByDescending(i => i.TotalQuantity)
                .Take(10) // limit results to top 10
                .ToList();

            return items;
        }

        // ====================================
        // ===           HELPERS            ===
        // ====================================
        // - for stuff like taking an OrderId and returning an Order object with the fields filled in
        public async Task<OrderDetailsDto> GetOrderById (int orderId)
        {
            var orderResponse = await _http.GetAsync($"/api/orders/{orderId}");
            orderResponse.EnsureSuccessStatusCode();
            var order = await orderResponse.Content.ReadFromJsonAsync<OrderDetailsDto>();
            if (order is null)
            {
                throw new InvalidOperationException($"Order {orderId} not found.");
            }
            return order;
        }

        // ====================================
        // === DATA TRANSFER OBJECTS (DTOs) ===
        // ====================================
        // for deserializing order details from Orders API
        // and for condensing data from multiple tables
        public class OrderDetailsDto
        {
            public int OrderId { get; set; }
            public int? UserId { get; set; }
            public string? Status { get; set; }
            public DateTime PlacedAt { get; set; }
            public DateTime? CompletedAt { get; set; }
            public decimal TotalAmount { get; set; }
            public List<OrderItemsDto> Items { get; set; } = new();
        }

        // !!! This is different from OrderItemDetailsDTO !!!
        // OrderItemsDto mirrors the "OrderItems" field in the Order model
        // OrderItemDetailsDto includes more info about item names and MenuIdItemIds for use in notifications
        public record OrderItemsDto
        {
            public int OrderItemId { get; init; }
            public int OrderId { get; init; }
            public int CombinationId { get; init; }
            public int Quantity { get; init; }
            public decimal UnitPrice { get; init; }
            public decimal LineTotal { get; init; }
        }



        // =======================
        // === TESTING METHODS ===
        // =======================

        // test notifications
        public async Task TestNotificationsAsync(string? notificationPref)
        {
            // create fake Order object
            var fakeOrder = new OrderDetailsDto
            {
                OrderId = 1,
                UserId = 1,
                Status = "Placed",
                PlacedAt = DateTime.UtcNow,
                TotalAmount = 9.99m,
                Items = new List<OrderItemsDto>
                {
                    new OrderItemsDto
                    {
                        OrderItemId = 1,
                        OrderId = 1,
                        CombinationId = 1,
                        Quantity = 1,
                        UnitPrice = 9.99m
                    }
                }
            };

            // create fake UserResponse object
            var fakeUser = new UserResponse
            {
                NotificationPref = notificationPref ?? "Email",
                PhoneNumber = _testToPhoneNumber,
                Email = _testToEmail
            };

            // create fake order item details
            var fakeItemDetails = new List<OrderItemDetailsDto>
            {
                new OrderItemDetailsDto
                {
                    MenuItemId = 1,
                    MenuItemName = "Test Latte",
                    Options = new List<string> { "Oat Milk", "Extra Shot" },
                    Quantity = 1,
                    UnitPrice = 9.99m,
                    LineTotal = 9.99m
                }
            };

            // create fake completed order details
            var fakeCompletedOrder = new OrderDetailsDto
            {
                OrderId = fakeOrder.OrderId,
                UserId = fakeOrder.UserId,
                Status = "Completed",
                PlacedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow,
                TotalAmount = fakeOrder.TotalAmount
            };

            // fake wait time
            double testWaitTime = 7.5;

            foreach (var notifier in _notifier)
            {
                await notifier.SendPlacedAsync(
                    fakeOrder,
                    fakeUser,
                    testWaitTime,
                    fakeItemDetails);

                await notifier.SendCompletionAsync(
                    fakeCompletedOrder,
                    fakeUser,
                    fakeItemDetails);
            }
        }

        // test wait time estimation with fake order
        public double TestWaitTimeEstimation()
        {
            // set fake order
            var fakeOrder = new OrderDetailsDto
            {
                OrderId = 999,
                UserId = null,
                Status = "Placed",
                PlacedAt = DateTime.UtcNow,
                TotalAmount = 18.97m,
                Items = new List<OrderItemsDto>
                {
                    new OrderItemsDto
                    {
                        OrderItemId = 1,
                        OrderId = 1,
                        CombinationId = 1,
                        Quantity = 1,
                        UnitPrice = 9.99m
                    }
                }
            };

            // set fake order items
            var fakeItems = new List<OrderItemDetailsDto>
            {
                new OrderItemDetailsDto
                {
                    MenuItemId = 1,
                    MenuItemName = "Sandwhich",
                    Quantity = 1,
                    UnitPrice = 9.99m,
                    LineTotal = 9.99m,
                    Options = new List<string> { "Extra chicken", "No beans" }
                },
                new OrderItemDetailsDto
                {
                    MenuItemId = 2,
                    MenuItemName = "Coffee",
                    Quantity = 1,
                    UnitPrice = 4.99m,
                    LineTotal = 4.99m,
                    Options = new List<string> { "Extra guac" }
                },
                new OrderItemDetailsDto
                {
                    MenuItemId = 3,
                    MenuItemName = "Fries",
                    Quantity = 1,
                    UnitPrice = 3.99m,
                    LineTotal = 3.99m
                }
            };
            double estimatedWaitTime = _estimator.Estimate(fakeOrder, fakeItems);
            return estimatedWaitTime;
        }
    }
}