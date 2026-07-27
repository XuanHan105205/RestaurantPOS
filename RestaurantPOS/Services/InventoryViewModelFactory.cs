using RestaurantPOS.ViewModels.Inventory;

namespace RestaurantPOS.Services
{
    public class InventoryViewModelFactory : IInventoryViewModelFactory
    {
        private readonly IDialogService _dialogService;

        public InventoryViewModelFactory(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        public InventoryViewModel Create()
        {
            var ingredientService = new IngredientService();
            var stockService = new StockService();
            var recipeService = new RecipeService();
            var dishService = new DishService();
            var employeeService = new EmployeeService();

            var ingredientVM = new IngredientViewModel(ingredientService, _dialogService);
            var stockReceiptVM = new StockReceiptViewModel(stockService, ingredientService, employeeService);
            var recipeMappingVM = new RecipeMappingViewModel(recipeService, ingredientService, dishService, _dialogService);

            return new InventoryViewModel(ingredientVM, stockReceiptVM, recipeMappingVM);
        }
    }
}
