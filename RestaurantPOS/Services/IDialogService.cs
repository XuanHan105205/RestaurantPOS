using RestaurantPOS.Models;

namespace RestaurantPOS.Services
{
    public interface IDialogService
    {
        void ShowMessage(string message, string title = "Thông báo");
        bool Confirm(string message, string title = "Xác nhận");
        bool? ShowOrderDetailPopup(DiningSession session, string tableName);
    }
}
