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

        public RecipeService(IRecipeRepository recipeRepository)
        {
            _recipeRepository = recipeRepository;
        }

        public List<Recipe> GetRecipesByDishId(int dishId)
        {
            if (dishId <= 0) return new List<Recipe>();
            return _recipeRepository.GetRecipesByDishId(dishId) ?? new List<Recipe>();
        }

        public Recipe GetRecipe(int dishId, int ingredientId)
        {
            if (dishId <= 0 || ingredientId <= 0) return null;
            return _recipeRepository.GetRecipe(dishId, ingredientId);
        }

        public bool AddOrUpdateRecipe(Recipe recipe)
        {
            if (recipe == null) return false;
            if (recipe.DishId <= 0 || recipe.IngredientId <= 0) return false;
            if (recipe.QuantityPerServing <= 0) return false;

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
            if (dishId <= 0 || ingredientId <= 0) return false;
            return _recipeRepository.DeleteRecipe(dishId, ingredientId);
        }
    }
}
