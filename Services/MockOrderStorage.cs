using WaitTimeTesting.Models;
using Microsoft.Extensions.Logging;
using WaitTimeTesting.Services;

namespace WaitTimeTesting.Services
{
    public class MockOrderStorage : IOrderStorage
    {
        private readonly ILogger<MockOrderStorage> _logger;

        public MockOrderStorage(ILogger<MockOrderStorage> logger)
        {
            _logger = logger;
        }

        public void StoreCompleted(Order order)
        {
            _logger.LogInformation($"[MOCK Storage] Stored completed order {order.Uid}.");
            // In real: Insert to DB
        }
    }
}