using System;
using System.Collections.Generic;
using System.Linq;
using RestaurantPOS.Models;
using RestaurantPOS.Repositories;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Inventory;
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

        public bool DeductStockForDish(int dishId, int quantity) => true;
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

    public class FakeEmployeeRepository : IEmployeeRepository
    {
        private readonly List<Employee> _data = new List<Employee>
        {
            new Employee { EmployeeId = 1, FullName = "Nguyễn Văn A", Username = "empA", Role = "admin", IsActive = true, PasswordHash = "123" }
        };

        public List<Employee> GetAll() => new List<Employee>(_data);
        public Employee? GetById(int id) => _data.FirstOrDefault(e => e.EmployeeId == id);
        public Employee? GetByUsername(string username) => _data.FirstOrDefault(e => e.Username == username);
        public bool Add(Employee entity) { _data.Add(entity); return true; }
        public bool Update(Employee entity) => true;
        public bool Delete(int id) => true;
    }

    public class FakeDishRepository : IDishRepository
    {
        private readonly List<Dish> _data = new List<Dish>
        {
            new Dish { DishId = 1, DishName = "Phở Bò", Price = 50000, AvailabilityStatus = "active" },
            new Dish { DishId = 2, DishName = "Bún Chả", Price = 45000, AvailabilityStatus = "active" }
        };

        public List<Dish> GetAll() => new List<Dish>(_data);
        public Dish? GetById(int id) => _data.FirstOrDefault(d => d.DishId == id);
        public bool Add(Dish entity) { _data.Add(entity); return true; }
        public bool Update(Dish entity) => true;
        public bool Delete(int id) => true;
        public List<Dish> GetActiveDishes() => _data.Where(d => d.AvailabilityStatus == "active").ToList();
    }

    public class FakeDialogService : IDialogService
    {
        public bool ConfirmResult { get; set; } = true;
        public bool WasConfirmCalled { get; private set; }

        public bool Confirm(string title, string message)
        {
            WasConfirmCalled = true;
            return ConfirmResult;
        }

        public void ShowMessage(string title, string message) { }
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

        [Theory]
        [InlineData(10.0, -1.0, false)] // minAlert = -1 biểu thị không đặt ngưỡng (null)
        [InlineData(10.0, 5.0, false)]  // tồn kho 10, ngưỡng 5 -> false
        [InlineData(5.0, 5.0, true)]    // tồn kho 5, ngưỡng 5 -> true
        [InlineData(3.0, 5.0, true)]    // tồn kho 3, ngưỡng 5 -> true
        public void Ingredient_IsLowStock_CalculatesCorrectly(double stockQtyVal, double minAlertVal, bool expectedIsLowStock)
        {
            decimal stockQty = (decimal)stockQtyVal;
            decimal? minAlert = minAlertVal < 0 ? null : (decimal?)minAlertVal;
            var ingredient = new Ingredient
            {
                IngredientName = "Test Item",
                Unit = "kg",
                StockQuantity = stockQty,
                MinStockAlert = minAlert
            };

            Assert.Equal(expectedIsLowStock, ingredient.IsLowStock);
        }

        [Fact]
        public void IngredientViewModel_Delete_UsesDialogService()
        {
            var ingRepo = new FakeIngredientRepository();
            ingRepo.Add(new Ingredient { IngredientId = 1, IngredientName = "Muối", Unit = "kg", StockQuantity = 10 });
            var ingService = new IngredientService(ingRepo);
            var dialogService = new FakeDialogService { ConfirmResult = true };

            var vm = new IngredientViewModel(ingService, dialogService);
            vm.SelectedIngredient = vm.Ingredients.First();

            vm.DeleteIngredientCommand.Execute(null);

            Assert.True(dialogService.WasConfirmCalled);
            Assert.Empty(vm.Ingredients);
        }

        [Fact]
        public void RecipeMappingViewModel_LoadDishes_DoesNotUseDbContextDirectly()
        {
            var recipeRepo = new FakeRecipeRepository();
            var recipeService = new RecipeService(recipeRepo);
            var ingRepo = new FakeIngredientRepository();
            var ingService = new IngredientService(ingRepo);
            var dishRepo = new FakeDishRepository();
            var dishService = new DishService(dishRepo);
            var dialogService = new FakeDialogService();

            var vm = new RecipeMappingViewModel(recipeService, ingService, dishService, dialogService);

            Assert.NotNull(vm.Dishes);
            Assert.Equal(2, vm.Dishes.Count);
        }

        [Fact]
        public void StockReceiptViewModel_LoadData_LoadsEmployeesViaService()
        {
            var stockRepo = new FakeStockReceiptRepository();
            stockRepo.Add(new StockReceipt { ReceiptId = 1, IngredientId = 1, Quantity = 10, ReceivedByEmployeeId = 1 });
            var stockService = new StockService(stockRepo);

            var ingRepo = new FakeIngredientRepository();
            ingRepo.Add(new Ingredient { IngredientId = 1, IngredientName = "Gạo", Unit = "kg", StockQuantity = 50 });
            var ingService = new IngredientService(ingRepo);

            var empRepo = new FakeEmployeeRepository();
            var empService = new EmployeeService(empRepo);

            var vm = new StockReceiptViewModel(stockService, ingService, empService);

            Assert.Single(vm.Receipts);
            Assert.Equal("Nguyễn Văn A", vm.Receipts[0].ReceivedByEmployeeName);
        }

        [Fact]
        public void InventoryViewModel_Constructor_AssignsSubViewModelsCorrectly()
        {
            var ingService = new IngredientService(new FakeIngredientRepository());
            var dialogService = new FakeDialogService();
            var ingVM = new IngredientViewModel(ingService, dialogService);

            var stockService = new StockService(new FakeStockReceiptRepository());
            var empService = new EmployeeService(new FakeEmployeeRepository());
            var stockVM = new StockReceiptViewModel(stockService, ingService, empService);

            var recipeService = new RecipeService(new FakeRecipeRepository());
            var dishService = new DishService(new FakeDishRepository());
            var recipeVM = new RecipeMappingViewModel(recipeService, ingService, dishService, dialogService);

            var inventoryVM = new InventoryViewModel(ingVM, stockVM, recipeVM);

            Assert.Same(ingVM, inventoryVM.IngredientVM);
            Assert.Same(stockVM, inventoryVM.StockReceiptVM);
            Assert.Same(recipeVM, inventoryVM.RecipeMappingVM);
        }

        [Fact]
        public void InventoryViewModelFactory_Create_ReturnsPopulatedInventoryViewModel()
        {
            var dialogService = new FakeDialogService();
            var factory = new InventoryViewModelFactory(dialogService);

            var resultVM = factory.Create();

            Assert.NotNull(resultVM);
            Assert.NotNull(resultVM.IngredientVM);
            Assert.NotNull(resultVM.StockReceiptVM);
            Assert.NotNull(resultVM.RecipeMappingVM);
        }
    }
}


