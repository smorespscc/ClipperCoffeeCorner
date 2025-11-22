using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClipperCoffeeCorner.Models
{
    [Table("OrderItem")]
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }

        public int OrderId { get; set; }        // FK → Order
        public int CombinationId { get; set; }  // FK → Combination

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }

        // Computed in SQL: Quantity * UnitPrice
        [Column(TypeName = "decimal(10,2)")]
        public decimal LineTotal { get; private set; }

        // Navigation
        public Order? Order { get; set; }
        public Combination? Combination { get; set; }
    }
}
