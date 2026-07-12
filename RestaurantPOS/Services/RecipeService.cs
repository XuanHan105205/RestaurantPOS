using System.Collections.Generic;
using RestaurantPOS.Models;
using RestaurantPOS.Repositories;

namespace RestaurantPOS.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly IRecipeRepository _recipeRepository;

        public RecipeService()
        {
            _recipeRepository = new RecipeRepository();
        }

        public List<Recipe> GetRecipesByDishId(int dishId)
        {
            return _recipeRepository.GetRecipesByDishId(dishId);
        }

        public Recipe GetRecipe(int dishId, int ingredientId)
        {
            return _recipeRepository.GetRecipe(dishId, ingredientId);
        }

        public bool AddOrUpdateRecipe(Recipe recipe)
        {
            var existing = _recipeRepository.GetRecipe(recipe.DishId, recipe.IngredientId);
            if (existing != null)
            {
                existing.QuantityPerServing = recipe.QuantityPerServing;
                return _recipeRepository.Update(existing);
            }
            else
            {
                return _recipeRepository.Add(recipe);
            }
        }

        public bool DeleteRecipe(int dishId, int ingredientId)
        {
            return _recipeRepository.DeleteRecipe(dishId, ingredientId);
        }
    }
}
