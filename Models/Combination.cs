using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClipperCoffeeCorner.Models
{
    [Table("Combination")]
    public class Combination
    {
        [Key]
        public int CombinationId { get; set; }

        public int MenuItemId { get; set; }   // FK → MenuItem

        [MaxLength(50)]
        public string? Code { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        public bool IsActive { get; set; }

        // navigation
        public MenuItem? MenuItem { get; set; }

        public ICollection<CombinationOption> CombinationOptions { get; set; }
            = new List<CombinationOption>();

        public ICollection<OrderItem> OrderItems { get; set; }
            = new List<OrderItem>();
    }
}