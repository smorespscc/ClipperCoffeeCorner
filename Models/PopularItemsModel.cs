namespace ClipperCoffeeCorner.Models
{
    public class PopularItemsModel
    {
        public required Guid MenuItemId { get; set; }
        public required string Name { get; set; }
        public required int OrderCount { get; set; }
    }
}