using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClipperCoffeeCorner.Models
{
    [Table("MenuCategory")]
    public class MenuCategory
    {
        [Key]
        public int MenuCategoryId { get; set; }

        public int? ParentCategoryId { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        // Navigation
        public MenuCategory? ParentCategory { get; set; }
        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    }
}
