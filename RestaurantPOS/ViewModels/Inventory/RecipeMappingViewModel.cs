using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using RestaurantPOS.Models;
using RestaurantPOS.MVVM;
using RestaurantPOS.Services;

namespace RestaurantPOS.ViewModels.Inventory
{
    public class RecipeMappingViewModel : ViewModelBase
    {
        private readonly IRecipeService _recipeService;
        private readonly IIngredientService _ingredientService;

        private List<Dish> _dishes;
        public List<Dish> Dishes
        {
            get => _dishes;
            set => SetProperty(ref _dishes, value);
        }

        private Dish _selectedDish;
        public Dish SelectedDish
        {
            get => _selectedDish;
            set
            {
                if (SetProperty(ref _selectedDish, value))
                {
                    LoadRecipeItems();
                }
            }
        }

        private ObservableCollection<RecipeItemDto> _recipeItems;
        public ObservableCollection<RecipeItemDto> RecipeItems
        {
            get => _recipeItems;
            set => SetProperty(ref _recipeItems, value);
        }

        private RecipeItemDto _selectedRecipeItem;
        public RecipeItemDto SelectedRecipeItem
        {
            get => _selectedRecipeItem;
            set
            {
                if (SetProperty(ref _selectedRecipeItem, value))
                {
                    LoadSelectedRecipeItemDetails();
                }
            }
        }

        private List<Ingredient> _ingredients;
        public List<Ingredient> Ingredients
        {
            get => _ingredients;
            set => SetProperty(ref _ingredients, value);
        }

        private Ingredient _selectedIngredient;
        public Ingredient SelectedIngredient
        {
            get => _selectedIngredient;
            set => SetProperty(ref _selectedIngredient, value);
        }

        private decimal _quantityPerServing;
        public decimal QuantityPerServing
        {
            get => _quantityPerServing;
            set => SetProperty(ref _quantityPerServing, value);
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private string _statusColor;
        public string StatusColor
        {
            get => _statusColor;
            set => SetProperty(ref _statusColor, value);
        }

        public bool IsRecipeItemSelected => SelectedRecipeItem != null;
        public bool IsDishSelected => SelectedDish != null;

        public ICommand LoadDishesCommand { get; }
        public ICommand SaveRecipeItemCommand { get; }
        public ICommand DeleteRecipeItemCommand { get; }
        public ICommand ClearFormCommand { get; }

        public RecipeMappingViewModel()
        {
            _recipeService = new RecipeService();
            _ingredientService = new IngredientService();
            RecipeItems = new ObservableCollection<RecipeItemDto>();

            LoadDishesCommand = new RelayCommand(LoadDishes);
            SaveRecipeItemCommand = new RelayCommand(ExecuteSave, () => IsDishSelected);
            DeleteRecipeItemCommand = new RelayCommand(ExecuteDelete, () => IsRecipeItemSelected);
            ClearFormCommand = new RelayCommand(ClearForm);

            LoadDishes();
        }

        private void LoadDishes()
        {
            try
            {
                using (var context = new Data.RestaurantPOSDbContext())
                {
                    Dishes = context.Dishes.Where(d => d.AvailabilityStatus == "active").OrderBy(d => d.DishName).ToList();
                }
                Ingredients = _ingredientService.GetAllIngredients().OrderBy(i => i.IngredientName).ToList();
                ShowStatus("Đã tải danh sách thực đơn.", true);
            }
            catch (Exception ex)
            {
                ShowStatus($"Lỗi tải thực đơn: {ex.Message}", false);
            }
        }

        private void LoadRecipeItems()
        {
            RecipeItems.Clear();
            ClearForm();
            OnPropertyChanged(nameof(IsDishSelected));
            CommandManager.InvalidateRequerySuggested();

            if (SelectedDish == null) return;

            try
            {
                var recipes = _recipeService.GetRecipesByDishId(SelectedDish.DishId);
                var dbIngredients = _ingredientService.GetAllIngredients().ToDictionary(i => i.IngredientId);

                var mapped = recipes.Select(r => new RecipeItemDto
                {
                    DishId = r.DishId,
                    IngredientId = r.IngredientId,
                    IngredientName = dbIngredients.TryGetValue(r.IngredientId, out var ing) ? ing.IngredientName : "Không xác định",
                    Unit = ing?.Unit ?? "",
                    QuantityPerServing = r.QuantityPerServing
                }).ToList();

                RecipeItems = new ObservableCollection<RecipeItemDto>(mapped);
            }
            catch (Exception ex)
            {
                ShowStatus($"Lỗi tải công thức: {ex.Message}", false);
            }
        }

        private void LoadSelectedRecipeItemDetails()
        {
            if (SelectedRecipeItem != null)
            {
                SelectedIngredient = Ingredients.FirstOrDefault(i => i.IngredientId == SelectedRecipeItem.IngredientId);
                QuantityPerServing = SelectedRecipeItem.QuantityPerServing;
            }
            else
            {
                SelectedIngredient = null;
                QuantityPerServing = 0;
            }
            OnPropertyChanged(nameof(IsRecipeItemSelected));
            CommandManager.InvalidateRequerySuggested();
        }

        private void ExecuteSave()
        {
            if (SelectedDish == null) return;

            if (SelectedIngredient == null)
            {
                ShowStatus("Vui lòng chọn nguyên liệu!", false);
                return;
            }
            if (QuantityPerServing <= 0)
            {
                ShowStatus("Định lượng nguyên liệu phải lớn hơn 0!", false);
                return;
            }

            try
            {
                var recipe = new Recipe
                {
                    DishId = SelectedDish.DishId,
                    IngredientId = SelectedIngredient.IngredientId,
                    QuantityPerServing = QuantityPerServing
                };

                if (_recipeService.AddOrUpdateRecipe(recipe))
                {
                    ShowStatus($"Cập nhật định lượng món '{SelectedDish.DishName}' thành công!", true);
                    LoadRecipeItems();
                }
                else
                {
                    ShowStatus("Cập nhật định lượng món thất bại!", false);
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"Lỗi hệ thống: {ex.Message}", false);
            }
        }

        private void ExecuteDelete()
        {
            if (SelectedDish == null || SelectedRecipeItem == null) return;

            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa định lượng nguyên liệu '{SelectedRecipeItem.IngredientName}' ra khỏi món '{SelectedDish.DishName}' không?", 
                "Xác nhận xóa định lượng", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (_recipeService.DeleteRecipe(SelectedDish.DishId, SelectedRecipeItem.IngredientId))
                    {
                        ShowStatus("Xóa định lượng nguyên liệu thành công!", true);
                        LoadRecipeItems();
                    }
                    else
                    {
                        ShowStatus("Xóa định lượng nguyên liệu thất bại!", false);
                    }
                }
                catch (Exception ex)
                {
                    ShowStatus($"Lỗi hệ thống: {ex.Message}", false);
                }
            }
        }

        private void ClearForm()
        {
            SelectedRecipeItem = null;
            SelectedIngredient = null;
            QuantityPerServing = 0;
        }

        private void ShowStatus(string message, bool isSuccess)
        {
            StatusMessage = message;
            StatusColor = isSuccess ? "#2ECC71" : "#E74C3C";
        }
    }

    public class RecipeItemDto
    {
        public int DishId { get; set; }
        public int IngredientId { get; set; }
        public string IngredientName { get; set; }
        public string Unit { get; set; }
        public decimal QuantityPerServing { get; set; }
    }
}
