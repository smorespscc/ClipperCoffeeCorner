using ClipperCoffeeCorner.Services.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;

namespace ClipperCoffeeCorner.Models.Domain
{
    /// <summary>
    /// Represents an order in the queue display.
    /// Used for showing customers their order status and wait time.
    /// 
    /// FUTURE DATABASE MAPPING:
    /// This will map to a QueueEntries table with columns:
    /// - Id (PK, int, identity)
    /// - OrderNumber (int, unique)
    /// - OrderId (FK to Orders, int, nullable)
    /// - ItemDescription (nvarchar(200))
    /// - Status (nvarchar(20)) - 'Queued', 'Preparing', 'Ready', 'Completed'
    /// - QueuePosition (int)
    /// - EstimatedWaitTime (int) - in minutes
    /// - AddedAt (datetime2)
    /// - UpdatedAt (datetime2)
    /// </summary>
    public class QueueEntry
    {
        /// <summary>
        /// Unique identifier for this queue entry
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Order number displayed to customer
        /// </summary>
        [Required]
        public int OrderNumber { get; set; }

        /// <summary>
        /// Reference to actual order (nullable for placeholder entries)
        /// </summary>
        public int? OrderId { get; set; }

        /// <summary>
        /// Brief description of order items
        /// (e.g., "Cappuccino", "Multiple Items")
        /// </summary>
        [Required]
        [MaxLength(200)]
        public required string ItemDescription { get; set; }

        /// <summary>
        /// Current status of the order
        /// </summary>
        [Required]
        public QueueStatus Status { get; set; } = QueueStatus.Queued;

        /// <summary>
        /// Position in queue (1 = first in line)
        /// </summary>
        public int QueuePosition { get; set; }

        /// <summary>
        /// Estimated wait time in minutes
        /// </summary>
        public int EstimatedWaitTime { get; set; }

        /// <summary>
        /// When this entry was added to queue
        /// </summary>
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When this entry was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Whether this is the current user's order
        /// (Used for highlighting in UI)
        /// </summary>
        public bool IsCurrentUserOrder { get; set; }

        // Navigation properties for EF Core (future use)
        // public virtual Order? Order { get; set; }

        /// <summary>
        /// Gets Bootstrap badge class for status
        /// </summary>
        public string GetStatusBadgeClass()
        {
            return Status switch
            {
                QueueStatus.Queued => "bg-secondary",
                QueueStatus.Preparing => "bg-warning text-dark",
                QueueStatus.Ready => "bg-success",
                QueueStatus.Completed => "bg-info",
                _ => "bg-secondary"
            };
        }

        /// <summary>
        /// Gets display text for status
        /// </summary>
        public string GetStatusDisplayText()
        {
            return Status.ToString();
        }

        /// <summary>
        /// Gets formatted order number for display
        /// </summary>
        public string GetFormattedOrderNumber()
        {
            return $"#{OrderNumber}";
        }
    }
}
