using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClipperCoffeeCorner.Models
{
    [Table("OptionGroup")]
    public class OptionGroup
    {
        [Key]
        public int OptionGroupId { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        // Navigation
        public ICollection<OptionValue> OptionValues { get; set; }
            = new List<OptionValue>();

        public ICollection<MenuItemOptionGroup> MenuItemOptionGroups { get; set; }
            = new List<MenuItemOptionGroup>();
    }
}