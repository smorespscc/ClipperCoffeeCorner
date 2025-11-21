using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClipperCoffeeCorner.Models
{
    [Table("OptionValue")]
    public class OptionValue
    {
        [Key]
        public int OptionValueId { get; set; }

        public int OptionGroupId { get; set; } // FK → OptionGroup

        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal ExtraPrice { get; set; }

        // Navigation
        public OptionGroup? OptionGroup { get; set; }

        public ICollection<CombinationOption> CombinationOptions { get; set; }
            = new List<CombinationOption>();
    }
}