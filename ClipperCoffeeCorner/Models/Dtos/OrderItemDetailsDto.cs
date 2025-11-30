using System.Collections.Generic;

namespace ClipperCoffeeCorner.Models
{
    public class OrderItemDetailsDto
    {
        public int MenuItemId { get; set; }
        public string MenuItemName { get; set; } = null!;
        public List<string> Options { get; set; } = new List<string>();
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}