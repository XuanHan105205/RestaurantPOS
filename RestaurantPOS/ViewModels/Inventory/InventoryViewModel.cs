using RestaurantPOS.MVVM;

namespace RestaurantPOS.ViewModels.Inventory
{
    public class InventoryViewModel : ViewModelBase
    {
        public IngredientViewModel IngredientVM { get; }
        public StockReceiptViewModel StockReceiptVM { get; }
        public RecipeMappingViewModel RecipeMappingVM { get; }

        public InventoryViewModel()
            : this(new IngredientViewModel(), new StockReceiptViewModel(), new RecipeMappingViewModel())
        {
        }

        public InventoryViewModel(
            IngredientViewModel ingredientVM,
            StockReceiptViewModel stockReceiptVM,
            RecipeMappingViewModel recipeMappingVM)
        {
            IngredientVM = ingredientVM;
            StockReceiptVM = stockReceiptVM;
            RecipeMappingVM = recipeMappingVM;
        }
    }
}
