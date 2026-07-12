using System;
using System.Collections.Generic;
using System.Linq;
using RestaurantPOS.Models;
using RestaurantPOS.Data;

namespace RestaurantPOS.Repositories
{
    public class RecipeRepository : BaseRepository<Recipe>, IRecipeRepository
    {
        public override List<Recipe> GetAll()
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.Recipes.ToList();
            }
        }

        public override Recipe GetById(int id)
        {
            throw new NotImplementedException("Use GetRecipe(dishId, ingredientId) for composite key lookup.");
        }

        public override bool Add(Recipe entity)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                context.Recipes.Add(entity);
                return context.SaveChanges() > 0;
            }
        }

        public override bool Update(Recipe entity)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                context.Recipes.Update(entity);
                return context.SaveChanges() > 0;
            }
        }

        public override bool Delete(int id)
        {
            throw new NotImplementedException("Use DeleteRecipe(dishId, ingredientId) for composite key delete.");
        }

        public List<Recipe> GetRecipesByDishId(int dishId)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.Recipes
                    .Where(r => r.DishId == dishId)
                    .ToList();
            }
        }

        public Recipe GetRecipe(int dishId, int ingredientId)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.Recipes
                    .FirstOrDefault(r => r.DishId == dishId && r.IngredientId == ingredientId);
            }
        }

        public bool DeleteRecipe(int dishId, int ingredientId)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                var recipe = context.Recipes
                    .FirstOrDefault(r => r.DishId == dishId && r.IngredientId == ingredientId);
                if (recipe != null)
                {
                    context.Recipes.Remove(recipe);
                    return context.SaveChanges() > 0;
                }
                return false;
            }
        }
    }
}
