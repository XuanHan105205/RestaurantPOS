using System.Collections.Generic;
using RestaurantPOS.Models;

namespace RestaurantPOS.Repositories
{
    public interface IIngredientRepository : IBaseRepository<Ingredient>
    {
        List<Ingredient> GetLowStockIngredients();
        bool DeductStockForDish(int dishId, int quantity);
    }
}
