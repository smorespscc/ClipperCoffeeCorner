using System.ComponentModel.DataAnnotations.Schema;

namespace ClipperCoffeeCorner.Models
{
    [Table("CombinationOption")]
    public class CombinationOption
    {
        // Composite PK: CombinationId + OptionValueId (configured in DbContext)

        public int CombinationId { get; set; }  // FK → Combination
        public int OptionValueId { get; set; }  // FK → OptionValue

        // Navigation
        public Combination? Combination { get; set; }
        public OptionValue? OptionValue { get; set; }
    }
}
