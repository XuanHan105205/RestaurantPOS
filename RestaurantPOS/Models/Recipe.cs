namespace RestaurantPOS.Models
{
    public class Recipe
    {
        public int DishId { get; set; }
        public int IngredientId { get; set; }
        public decimal QuantityPerServing { get; set; }
    }
}
