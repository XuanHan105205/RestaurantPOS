using System.Collections.Generic;
using RestaurantPOS.Models;
using RestaurantPOS.Repositories;

namespace RestaurantPOS.Services
{
    public class IngredientService : IIngredientService
    {
        private readonly IIngredientRepository _ingredientRepository;

        public IngredientService()
        {
            _ingredientRepository = new IngredientRepository();
        }

        public List<Ingredient> GetAllIngredients()
        {
            return _ingredientRepository.GetAll();
        }

        public Ingredient GetIngredientById(int id)
        {
            return _ingredientRepository.GetById(id);
        }

        public bool AddIngredient(Ingredient ingredient)
        {
            return _ingredientRepository.Add(ingredient);
        }

        public bool UpdateIngredient(Ingredient ingredient)
        {
            return _ingredientRepository.Update(ingredient);
        }

        public bool DeleteIngredient(int id)
        {
            return _ingredientRepository.Delete(id);
        }

        public List<Ingredient> GetLowStockIngredients()
        {
            return _ingredientRepository.GetLowStockIngredients();
        }
    }
}
