namespace RestaurantPOS.Models
{
    public class Dish
    {
        public int DishId { get; set; }
        public string DishName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int? CategoryId { get; set; }
        public string AvailabilityStatus { get; set; } = "active"; // 'active', 'discontinued'
        public string? ImageUrl { get; set; }
    }
}
