using System.Collections.Generic;
using RestaurantPOS.Models;

namespace RestaurantPOS.Repositories
{
    public interface IRecipeRepository : IBaseRepository<Recipe>
    {
        List<Recipe> GetRecipesByDishId(int dishId);
        Recipe GetRecipe(int dishId, int ingredientId);
        bool DeleteRecipe(int dishId, int ingredientId);
    }
}
