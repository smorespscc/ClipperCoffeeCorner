using ClipperCoffeeCorner.Models;
using ClipperCoffeeCorner.Options;

namespace ClipperCoffeeCorner.Services
{
    public interface IWaitTimeEstimator
    {
        double Estimate(Order order);
        void AddCompletedForTraining(Order order);
    }

    public class WaitTimeEstimator : IWaitTimeEstimator
    {
        private readonly ILogger<WaitTimeEstimator> _logger;

        public WaitTimeEstimator(ILogger<WaitTimeEstimator> logger)
        {
            _logger = logger;
        }

        // Main estimation method. This is called when a new order is placed with the Order object and is expected to return an estimated wait time
        // the "features" part could be removed and this could just return a double.
        // The ML estimation stuff is split into different methods here, but it can be changed however you want as long as this method returns the estimated wait time.
        public double Estimate(Order order)
        {
            var estimatedWaitTime = 15.0; // Placeholder fixed estimate





            return estimatedWaitTime;
        }


        // Called when an order is completed. Don't know if you need this but might be useful.
        // Isn't expected to return anything.
        public void AddCompletedForTraining(Order order)
        {



        }

    }

}