using System.ComponentModel.DataAnnotations.Schema;

namespace ClipperCoffeeCorner.Models
{
    [Table("MenuItemOptionGroup")]
    public class MenuItemOptionGroup
    {
        // Composite key: MenuItemId + OptionGroupId (configured in DbContext)

        public int MenuItemId { get; set; }     // FK → MenuItem
        public int OptionGroupId { get; set; }  // FK → OptionGroup

        public bool IsRequired { get; set; }
        public int MinChoices { get; set; }
        public int MaxChoices { get; set; }

        // Navigation
        public MenuItem? MenuItem { get; set; }
        public OptionGroup? OptionGroup { get; set; }
    }
}