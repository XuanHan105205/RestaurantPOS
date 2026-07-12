using System.Collections.Generic;
using RestaurantPOS.Models;

namespace RestaurantPOS.Services
{
    public interface IIngredientService
    {
        List<Ingredient> GetAllIngredients();
        Ingredient GetIngredientById(int id);
        bool AddIngredient(Ingredient ingredient);
        bool UpdateIngredient(Ingredient ingredient);
        bool DeleteIngredient(int id);
        List<Ingredient> GetLowStockIngredients();
    }
}
