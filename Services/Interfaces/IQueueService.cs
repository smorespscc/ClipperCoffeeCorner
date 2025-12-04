using ClipperCoffeeCorner.Models.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ClipperCoffeeCorner.Services.Interfaces
{
    /// <summary>
    /// Business logic for queue management and display.
    /// Handles queue position, wait time estimation, and queue visualization.
    /// </summary>
    public interface IQueueService
    {
        // ==================== QUEUE MANAGEMENT ====================
        
        /// <summary>
        /// Gets current queue entries for display
        /// </summary>
        Task<List<QueueEntry>> GetCurrentQueueAsync();
        
        /// <summary>
        /// Gets customer's position in queue
        /// </summary>
        Task<int?> GetCustomerQueuePositionAsync(int orderNumber);
        
        /// <summary>
        /// Estimates wait time for an order
        /// </summary>
        Task<int> EstimateWaitTimeAsync(int orderNumber);
        
        /// <summary>
        /// Adds an order to the queue
        /// </summary>
        Task<bool> AddToQueueAsync(int orderNumber, string itemDescription);
        
        /// <summary>
        /// Updates order status in queue
        /// </summary>
        Task<bool> UpdateOrderStatusAsync(int orderNumber, QueueStatus status);
        
        /// <summary>
        /// Removes order from queue (completed or cancelled)
        /// </summary>
        Task<bool> RemoveFromQueueAsync(int orderNumber);

        // ==================== QUEUE DISPLAY ====================
        
        /// <summary>
        /// Generates placeholder queue for demonstration
        /// </summary>
        Task<List<QueueEntry>> GeneratePlaceholderQueueAsync(int baseOrderNumber = 200);
        
        /// <summary>
        /// Checks if order is ready
        /// </summary>
        Task<bool> IsOrderReadyAsync(int orderNumber);
    }

    /// <summary>
    /// Queue entry status
    /// </summary>
    public enum QueueStatus
    {
        Queued,
        Preparing,
        Ready,
        Completed
    }
}
