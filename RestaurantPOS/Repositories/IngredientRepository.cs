using System.Collections.Generic;
using System.Linq;
using RestaurantPOS.Models;
using RestaurantPOS.Data;

namespace RestaurantPOS.Repositories
{
    public class IngredientRepository : BaseRepository<Ingredient>, IIngredientRepository
    {
        public override List<Ingredient> GetAll()
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.Ingredients.ToList();
            }
        }

        public override Ingredient GetById(int id)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.Ingredients.Find(id);
            }
        }

        public override bool Add(Ingredient entity)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                context.Ingredients.Add(entity);
                return context.SaveChanges() > 0;
            }
        }

        public override bool Update(Ingredient entity)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                context.Ingredients.Update(entity);
                return context.SaveChanges() > 0;
            }
        }

        public override bool Delete(int id)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                var ingredient = context.Ingredients.Find(id);
                if (ingredient != null)
                {
                    context.Ingredients.Remove(ingredient);
                    return context.SaveChanges() > 0;
                }
                return false;
            }
        }

        public List<Ingredient> GetLowStockIngredients()
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.Ingredients
                    .Where(i => i.MinStockAlert.HasValue && i.StockQuantity <= i.MinStockAlert.Value)
                    .ToList();
            }
        }

        public void DeductStockForDish(int dishId, int quantity, RestaurantPOSDbContext context)
        {
            var recipes = context.Recipes.Where(r => r.DishId == dishId).ToList();
            foreach (var recipe in recipes)
            {
                var ingredient = context.Ingredients.Find(recipe.IngredientId);
                if (ingredient != null)
                {
                    ingredient.StockQuantity -= recipe.QuantityPerServing * quantity;
                    if (ingredient.StockQuantity < 0)
                    {
                        ingredient.StockQuantity = 0;
                    }
                    context.Ingredients.Update(ingredient);
                }
            }
        }
    }
}
