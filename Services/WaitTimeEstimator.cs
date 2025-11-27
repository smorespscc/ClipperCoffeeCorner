using ClipperCoffeeCorner.Models;
using static ClipperCoffeeCorner.Services.WaitTimeNotificationService;

namespace ClipperCoffeeCorner.Services
{
    public interface IWaitTimeEstimator
    {
        double Estimate(Order order, List<OrderItemDetailsDto> itemDetails);
        void AddCompletedForTraining(OrderDetailsDto order, List<OrderItemDetailsDto> itemDetails);
    }

    public class WaitTimeEstimator : IWaitTimeEstimator
    {
        private readonly ILogger<WaitTimeEstimator> _logger;

        public WaitTimeEstimator(ILogger<WaitTimeEstimator> logger)
        {
            _logger = logger;
        }

        // Main estimation method. This is called when a new order is placed with the Order object and is expected to return an estimated wait time
        public double Estimate(Order order, List<OrderItemDetailsDto> itemDetails)
        {
            // Create a comma-separated concatenated string of MenuItemId values from the provided itemDetails.
            // If itemDetails is null or empty, produce an empty string.
            var menuItemIds = itemDetails == null || itemDetails.Count == 0
                ? string.Empty
                : string.Join(",", itemDetails.Select(i => i is null ? string.Empty : i.MenuItemId.ToString()));

            _logger.LogDebug("Concatenated MenuItemId string: {MenuItemIds}", menuItemIds);

            //Load sample data
            var sampleData = new MLModel.ModelInput()
            {
                Col0 = menuItemIds,
            };

            //Load model and predict output
            var result = MLModel.Predict(sampleData);

            double estimatedWaitTime = result.Score / 60.0;

            return estimatedWaitTime;
        }


        // Called when an order is completed. Don't know if you need this but might be useful.
        // Isn't expected to return anything.
        public void AddCompletedForTraining(OrderDetailsDto order, List<OrderItemDetailsDto> itemDetails)
        {



        }

    }

}