using RestaurantPOS.MVVM;

namespace RestaurantPOS.ViewModels.Inventory
{
    public class InventoryViewModel : ViewModelBase
    {
        public IngredientViewModel IngredientVM { get; }
        public StockReceiptViewModel StockReceiptVM { get; }
        public RecipeMappingViewModel RecipeMappingVM { get; }

        public InventoryViewModel()
        {
            IngredientVM = new IngredientViewModel();
            StockReceiptVM = new StockReceiptViewModel();
            RecipeMappingVM = new RecipeMappingViewModel();
        }
    }
}
