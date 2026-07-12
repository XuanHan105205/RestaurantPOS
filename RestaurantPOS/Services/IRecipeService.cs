using System.Collections.Generic;
using RestaurantPOS.Models;

namespace RestaurantPOS.Services
{
    public interface IRecipeService
    {
        List<Recipe> GetRecipesByDishId(int dishId);
        Recipe GetRecipe(int dishId, int ingredientId);
        bool AddOrUpdateRecipe(Recipe recipe);
        bool DeleteRecipe(int dishId, int ingredientId);
    }
}
