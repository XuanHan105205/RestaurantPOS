using System;
using System.Collections.Generic;
using RestaurantPOS.Models;
using RestaurantPOS.Repositories;
using RestaurantPOS.Services;
using Xunit;

namespace RestaurantPOS.Tests
{
    public class FakeIngredientRepository : IIngredientRepository
    {
        private readonly List<Ingredient> _data = new List<Ingredient>();

        public List<Ingredient> GetAll() => new List<Ingredient>(_data);

        public Ingredient? GetById(int id) => _data.Find(i => i.IngredientId == id);

        public bool Add(Ingredient entity)
        {
            if (entity.IngredientId <= 0) entity.IngredientId = _data.Count + 1;
            _data.Add(entity);
            return true;
        }

        public bool Update(Ingredient entity)
        {
            var idx = _data.FindIndex(i => i.IngredientId == entity.IngredientId);
            if (idx >= 0)
            {
                _data[idx] = entity;
                return true;
            }
            return false;
        }

        public bool Delete(int id)
        {
            return _data.RemoveAll(i => i.IngredientId == id) > 0;
        }

        public List<Ingredient> GetLowStockIngredients()
        {
            return _data.FindAll(i => i.MinStockAlert.HasValue && i.StockQuantity <= i.MinStockAlert.Value);
        }

        public void DeductStockForDish(int dishId, int quantity, Data.RestaurantPOSDbContext context)
        {
        }
    }

    public class FakeStockReceiptRepository : IStockReceiptRepository
    {
        private readonly List<StockReceipt> _data = new List<StockReceipt>();

        public List<StockReceipt> GetAll() => new List<StockReceipt>(_data);

        public StockReceipt? GetById(int id) => _data.Find(r => r.ReceiptId == id);

        public bool Add(StockReceipt entity)
        {
            if (entity.ReceiptId <= 0) entity.ReceiptId = _data.Count + 1;
            _data.Add(entity);
            return true;
        }

        public bool Update(StockReceipt entity) => true;

        public bool Delete(int id) => true;

        public List<StockReceipt> GetReceiptsByIngredientId(int ingredientId)
        {
            return _data.FindAll(r => r.IngredientId == ingredientId);
        }
    }

    public class FakeRecipeRepository : IRecipeRepository
    {
        private readonly List<Recipe> _data = new List<Recipe>();

        public List<Recipe> GetAll() => new List<Recipe>(_data);

        public Recipe? GetById(int id) => null;

        public Recipe? GetRecipe(int dishId, int ingredientId) => _data.Find(r => r.DishId == dishId && r.IngredientId == ingredientId);

        public List<Recipe> GetRecipesByDishId(int dishId) => _data.FindAll(r => r.DishId == dishId);

        public bool Add(Recipe entity)
        {
            _data.Add(entity);
            return true;
        }

        public bool Update(Recipe entity)
        {
            var existing = GetRecipe(entity.DishId, entity.IngredientId);
            if (existing != null)
            {
                existing.QuantityPerServing = entity.QuantityPerServing;
                return true;
            }
            return false;
        }

        public bool Delete(int id) => true;

        public bool DeleteRecipe(int dishId, int ingredientId)
        {
            return _data.RemoveAll(r => r.DishId == dishId && r.IngredientId == ingredientId) > 0;
        }
    }

    public class InventoryServiceTests
    {
        [Fact]
        public void AddIngredient_ValidInput_ReturnsTrue()
        {
            var repo = new FakeIngredientRepository();
            var service = new IngredientService(repo);
            var ingredient = new Ingredient
            {
                IngredientName = "Thịt bò",
                Unit = "kg",
                StockQuantity = 10,
                MinStockAlert = 2
            };

            bool result = service.AddIngredient(ingredient);

            Assert.True(result);
            Assert.Single(service.GetAllIngredients());
        }

        [Fact]
        public void AddIngredient_EmptyName_ReturnsFalse()
        {
            var repo = new FakeIngredientRepository();
            var service = new IngredientService(repo);
            var ingredient = new Ingredient
            {
                IngredientName = "   ",
                Unit = "kg",
                StockQuantity = 10
            };

            bool result = service.AddIngredient(ingredient);

            Assert.False(result);
        }

        [Fact]
        public void AddIngredient_NegativeStock_ReturnsFalse()
        {
            var repo = new FakeIngredientRepository();
            var service = new IngredientService(repo);
            var ingredient = new Ingredient
            {
                IngredientName = "Đường",
                Unit = "kg",
                StockQuantity = -5
            };

            bool result = service.AddIngredient(ingredient);

            Assert.False(result);
        }

        [Fact]
        public void AddIngredient_DuplicateName_ReturnsFalse()
        {
            var repo = new FakeIngredientRepository();
            var service = new IngredientService(repo);
            service.AddIngredient(new Ingredient { IngredientName = "Hành lá", Unit = "kg", StockQuantity = 5 });

            var duplicate = new Ingredient { IngredientName = "HÀNH LÁ", Unit = "g", StockQuantity = 10 };
            bool result = service.AddIngredient(duplicate);

            Assert.False(result);
        }

        [Fact]
        public void AddStockReceipt_InvalidQuantity_ReturnsFalse()
        {
            var repo = new FakeStockReceiptRepository();
            var service = new StockService(repo);
            var receipt = new StockReceipt
            {
                IngredientId = 1,
                Quantity = 0,
                UnitCost = 50000
            };

            bool result = service.AddStockReceipt(receipt);

            Assert.False(result);
        }

        [Fact]
        public void AddStockReceipt_ValidReceipt_ReturnsTrue()
        {
            var repo = new FakeStockReceiptRepository();
            var service = new StockService(repo);
            var receipt = new StockReceipt
            {
                IngredientId = 1,
                Quantity = 20,
                UnitCost = 50000
            };

            bool result = service.AddStockReceipt(receipt);

            Assert.True(result);
            Assert.Single(service.GetAllReceipts());
        }

        [Fact]
        public void AddOrUpdateRecipe_ValidRecipe_ReturnsTrue()
        {
            var repo = new FakeRecipeRepository();
            var service = new RecipeService(repo);
            var recipe = new Recipe
            {
                DishId = 1,
                IngredientId = 2,
                QuantityPerServing = 0.5m
            };

            bool result = service.AddOrUpdateRecipe(recipe);

            Assert.True(result);
            Assert.Single(service.GetRecipesByDishId(1));
        }

        [Fact]
        public void AddOrUpdateRecipe_InvalidQuantity_ReturnsFalse()
        {
            var repo = new FakeRecipeRepository();
            var service = new RecipeService(repo);
            var recipe = new Recipe
            {
                DishId = 1,
                IngredientId = 2,
                QuantityPerServing = 0
            };

            bool result = service.AddOrUpdateRecipe(recipe);

            Assert.False(result);
        }

        [Fact]
        public void DeleteRecipe_ValidIds_ReturnsTrue()
        {
            var repo = new FakeRecipeRepository();
            var service = new RecipeService(repo);
            service.AddOrUpdateRecipe(new Recipe { DishId = 1, IngredientId = 2, QuantityPerServing = 1.0m });

            bool result = service.DeleteRecipe(1, 2);

            Assert.True(result);
            Assert.Empty(service.GetRecipesByDishId(1));
        }
    }
}
