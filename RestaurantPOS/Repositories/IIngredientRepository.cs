using System.Collections.Generic;
using RestaurantPOS.Models;
using RestaurantPOS.Data;

namespace RestaurantPOS.Repositories
{
    public interface IIngredientRepository : IBaseRepository<Ingredient>
    {
        List<Ingredient> GetLowStockIngredients();
        void DeductStockForDish(int dishId, int quantity, RestaurantPOSDbContext context);
    }
}
