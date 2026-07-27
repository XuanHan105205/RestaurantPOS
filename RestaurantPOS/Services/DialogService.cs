using System.Windows;
using RestaurantPOS.Models;
using RestaurantPOS.ViewModels.Waiter;
using RestaurantPOS.Views.Waiter;

namespace RestaurantPOS.Services
{
    public class DialogService : IDialogService
    {
        private readonly IOrderService _orderService;

        public DialogService(IOrderService? orderService = null)
        {
            _orderService = orderService ?? new OrderService();
        }

        public void ShowMessage(string message, string title = "Thông báo")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public bool Confirm(string message, string title = "Xác nhận")
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

        public bool? ShowOrderDetailPopup(DiningSession session, string tableName)
        {
            var viewModel = new OrderDetailPopupViewModel(session, tableName, _orderService, this);
            var popup = new OrderDetailPopup(viewModel);

            if (Application.Current?.MainWindow != null)
            {
                popup.Owner = Application.Current.MainWindow;
            }

            return popup.ShowDialog();
        }
    }
}
