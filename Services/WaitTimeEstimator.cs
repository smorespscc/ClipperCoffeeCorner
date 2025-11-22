using ClipperCoffeeCorner.Models;
using static ClipperCoffeeCorner.Services.WaitTimeNotificationService;

namespace ClipperCoffeeCorner.Services
{
    public interface IWaitTimeEstimator
    {
        double Estimate(Order order);
        void AddCompletedForTraining(OrderDetailsDto order);
    }

    public class WaitTimeEstimator : IWaitTimeEstimator
    {
        private readonly ILogger<WaitTimeEstimator> _logger;

        public WaitTimeEstimator(ILogger<WaitTimeEstimator> logger)
        {
            _logger = logger;
        }

        // Main estimation method. This is called when a new order is placed with the Order object and is expected to return an estimated wait time
        public double Estimate(Order order)
        {
            var estimatedWaitTime = 15.0; // Placeholder fixed estimate





            return estimatedWaitTime;
        }


        // Called when an order is completed. Don't know if you need this but might be useful.
        // Isn't expected to return anything.
        public void AddCompletedForTraining(OrderDetailsDto order)
        {



        }

    }

}