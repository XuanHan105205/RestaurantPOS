using RestaurantPOS.ViewModels.Inventory;

namespace RestaurantPOS.Services
{
    public interface IInventoryViewModelFactory
    {
        InventoryViewModel Create();
    }
}
