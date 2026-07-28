using RestaurantPOS.Models;

namespace RestaurantPOS.Services
{
    public interface IOrderDetailDialogService
    {
        void Show(DiningSession session, string tableName);
    }
}
