namespace ClipperCoffeeCorner.Models
{
    public class PopularItemDto
    {
        public int MenuItemId { get; set; }
        public string MenuItemName { get; set; } = null!;
        public int MenuItemCategoryId { get; set; }
        public int TotalQuantity { get; set; }
        public int OrderCount { get; set; }
    }
}