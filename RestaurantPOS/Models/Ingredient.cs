namespace RestaurantPOS.Models
{
    public class Ingredient
    {
        public int IngredientId { get; set; }
        public string IngredientName { get; set; }
        public string Unit { get; set; }
        public decimal StockQuantity { get; set; }
        public decimal? MinStockAlert { get; set; }
    }
}
