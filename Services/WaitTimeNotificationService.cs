using ClipperCoffeeCorner.Models;
using ClipperCoffeeCorner.Controllers;


namespace ClipperCoffeeCorner.Services
{
    public class WaitTimeNotificationService
    {
        private readonly IWaitTimeEstimator _estimator;
        private readonly IEnumerable<INotificationService> _notifier;
        private readonly ILogger<WaitTimeNotificationService> _logger;

        private readonly HttpClient _http;

        public WaitTimeNotificationService(
            IWaitTimeEstimator estimator,
            IEnumerable<INotificationService> notifier,
            ILogger<WaitTimeNotificationService> logger,
            IHttpClientFactory factory)
        {
            _estimator = estimator;
            _notifier = notifier;
            _logger = logger;
            _http = factory.CreateClient();
            _http.BaseAddress = new Uri("https://localhost:7138"); // insert app URL when deployed
        }

        // still have no idea what information this is getting or if we are responsible for adding new orders to the DB
        // assuming we get an Order object with all necessary info filled out
        public async Task<double> ProcessNewOrder(Order order)
        {
            ArgumentNullException.ThrowIfNull(order);

            // ask ML estimation service for wait time (can change this depending on what ML service actually needs)
            double estimatedWaitTime = _estimator.Estimate(order);

            // add to orders table
            // create request according to OrdersController CreateOrderRequest DTO
            var request = new
            {
                userId = order.UserId.HasValue ? order.UserId : null,
                items = order.OrderItems.Select(oi => new
                {
                    combinationId = oi.CombinationId,
                    quantity = oi.Quantity
                }).ToList()
            };

            // POST to OrdersController CreateOrder endpoint
            var response = await _http.PostAsJsonAsync("/api/orders", request);

            if (order.UserId.HasValue)
            {
                // query users table to get email, phone number, and notification preference
                var userResponse = await _http.GetAsync($"/api/users/{order.UserId}");
                var user = await userResponse.Content.ReadFromJsonAsync<UserResponse>();

                // send confirmation SMS or Email
                foreach (var notifier in _notifier)
                {
                    #pragma warning disable CS8604 // Possible null reference argument.
                    await notifier.SendPlacedAsync(order, user, estimatedWaitTime);
                    #pragma warning restore CS8604 // Possible null reference argument.
                }
            }

            return estimatedWaitTime;
        }

        public async Task<OrderDetailsDto> CompleteOrderAsync(int orderId)
        {
            // create request to update order (don't think we are even doing this part but whatever)
            var request = new
            {
                status = "Completed"
            };

            // call api PUT /api/orders/{orderId}/status
            var response = await _http.PutAsJsonAsync($"/api/orders/{orderId}/status", request);

            // get updated order
            var orderResponse = await _http.GetAsync($"/api/orders/{orderId}");
            if (!orderResponse.IsSuccessStatusCode)
            {
                var error = await orderResponse.Content.ReadAsStringAsync();
                throw new Exception($"Failed to get order: {error}");
            }

            var order = await orderResponse.Content.ReadFromJsonAsync<OrderDetailsDto>();

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
                    await notifier.SendCompletionAsync(order, user);
                }
            }

            // give to ML training service. Might not need this.
            if (order is not null)
            {
                _estimator.AddCompletedForTraining(order);
                return order;
            }

            throw new InvalidOperationException($"Order {orderId} not found.");
        }


        // return type could be changed to whatever is needed
        // uses 100 most recent orders
        // doesn't work yet
        public async Task<List<PopularItemsModel>> GetPopularItemsAsync(int? menuCategory)
        {
            var response = await _http.GetAsync($"/api/orders/recent?n=100");

            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to retrieve recent orders.");

            var orders = await response.Content.ReadFromJsonAsync<List<OrderDetailsDto>>();

            if (orders == null)
                throw new Exception("Orders returned null from API.");

            // Process list of orders to determine most ordered items

            // Create a list of item objects. Maybe make a more simple DTO based off of the MenuItem model
            // Should only need to return a small subset of the MenuItem properties, and maybe only MenuItemId and just let whoever is calling this endpoint handle getting the rest of the info

            // placeholder return value
            return new List<PopularItemsModel>();
        }

        // DTOs for deserializing order details from Orders API
        public class OrderDetailsDto
        {
            public int OrderId { get; set; }
            public int? UserId { get; set; }
            public string? Status { get; set; }
            public DateTime PlacedAt { get; set; }
            public DateTime? CompletedAt { get; set; }
            public decimal TotalAmount { get; set; }
            public List<OrderItemDetailsDto> Items { get; set; } = new();
        }

        public class OrderItemDetailsDto
        {
            public int OrderItemId { get; set; }
            public int CombinationId { get; set; }
            public string? DrinkName { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal LineTotal { get; set; }
        }

        // =======================
        // === TESTING METHODS ===
        // =======================
        public async Task TestNotificationsAsync(Order order, UserResponse user)
        {
            double testWaitTime = 7.5;

            foreach (var notifier in _notifier)
            {
                await notifier.SendPlacedAsync(order, user, testWaitTime);

                await notifier.SendCompletionAsync(
                    new OrderDetailsDto { OrderId = order.OrderId },
                    user
                );
            }
        }
    }
}