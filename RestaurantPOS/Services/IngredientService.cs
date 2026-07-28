using System;
using System.Collections.Generic;
using System.Linq;
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

        public IngredientService(IIngredientRepository ingredientRepository)
        {
            _ingredientRepository = ingredientRepository;
        }

        public List<Ingredient> GetAllIngredients()
        {
            return _ingredientRepository.GetAll() ?? new List<Ingredient>();
        }

        public Ingredient? GetIngredientById(int id)
        {
            return _ingredientRepository.GetById(id);
        }

        public bool AddIngredient(Ingredient ingredient)
        {
            if (!ValidateIngredient(ingredient, out _))
            {
                return false;
            }

            // Kiểm tra trùng tên nguyên liệu
            var existing = GetAllIngredients().FirstOrDefault(i => 
                i.IngredientName.Equals(ingredient.IngredientName.Trim(), StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                return false;
            }

            ingredient.IngredientName = ingredient.IngredientName.Trim();
            ingredient.Unit = ingredient.Unit.Trim();
            return _ingredientRepository.Add(ingredient);
        }

        public bool UpdateIngredient(Ingredient ingredient)
        {
            if (!ValidateIngredient(ingredient, out _))
            {
                return false;
            }

            // Kiểm tra trùng tên với nguyên liệu khác
            var existing = GetAllIngredients().FirstOrDefault(i => 
                i.IngredientId != ingredient.IngredientId && 
                i.IngredientName.Equals(ingredient.IngredientName.Trim(), StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                return false;
            }

            ingredient.IngredientName = ingredient.IngredientName.Trim();
            ingredient.Unit = ingredient.Unit.Trim();
            return _ingredientRepository.Update(ingredient);
        }

        public bool DeleteIngredient(int id)
        {
            if (id <= 0) return false;
            return _ingredientRepository.Delete(id);
        }

        public List<Ingredient> GetLowStockIngredients()
        {
            return _ingredientRepository.GetLowStockIngredients() ?? new List<Ingredient>();
        }

        public bool DeductStockForDish(int dishId, int quantity)
        {
            if (dishId <= 0 || quantity <= 0)
            {
                return false;
            }

            return _ingredientRepository.DeductStockForDish(dishId, quantity);
        }

        public bool ValidateIngredient(Ingredient ingredient, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (ingredient == null)
            {
                errorMessage = "Dữ liệu nguyên liệu không hợp lệ.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(ingredient.IngredientName))
            {
                errorMessage = "Tên nguyên liệu không được để trống.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(ingredient.Unit))
            {
                errorMessage = "Đơn vị tính không được để trống.";
                return false;
            }

            if (ingredient.StockQuantity < 0)
            {
                errorMessage = "Số lượng tồn kho không được âm.";
                return false;
            }

            if (ingredient.MinStockAlert.HasValue && ingredient.MinStockAlert.Value < 0)
            {
                errorMessage = "Mức cảnh báo tồn kho tối thiểu không được âm.";
                return false;
            }

            return true;
        }
    }
}
