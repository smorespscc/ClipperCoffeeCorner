using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClipperCoffeeCorner.Models
{
    [Table("MenuItem")]
    public class MenuItem
    {
        [Key]
        public int MenuItemId { get; set; }

        public int MenuCategoryId { get; set; }  // FK → MenuCategory

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10,2)")]
        public decimal BasePrice { get; set; }

        public bool IsActive { get; set; }

        [MaxLength(400)]
        public string? Description { get; set; }

        // Navigation
        public MenuCategory? MenuCategory { get; set; }

        public ICollection<MenuItemOptionGroup> MenuItemOptionGroups { get; set; }
            = new List<MenuItemOptionGroup>();

        public ICollection<Combination> Combinations { get; set; }
            = new List<Combination>();
    }
}