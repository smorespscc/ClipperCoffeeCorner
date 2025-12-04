using ClipperCoffeeCorner.Models.Domain;
using System.Collections.Generic;

namespace ClipperCoffeeCorner.Models.ViewModels
{
    public class QueueViewModel
    {
        public List<QueueEntry> QueueEntries { get; set; } = new List<QueueEntry>();
        public int? UserOrderNumber { get; set; }
        public int? UserQueuePosition { get; set; }
        public int? EstimatedWaitTime { get; set; }
        public bool HasUserOrder => UserOrderNumber.HasValue;
    }
}
