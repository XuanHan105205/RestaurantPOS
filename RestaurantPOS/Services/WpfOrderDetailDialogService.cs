using System.Windows;
using RestaurantPOS.Models;
using RestaurantPOS.ViewModels.Waiter;
using RestaurantPOS.Views.Waiter;

namespace RestaurantPOS.Services
{
    public class WpfOrderDetailDialogService : IOrderDetailDialogService
    {
        private readonly IDialogService _dialogService;

        public WpfOrderDetailDialogService(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        public void Show(DiningSession session, string tableName)
        {
            var viewModel = new OrderDetailPopupViewModel(
                session,
                tableName,
                new OrderService(),
                _dialogService);
            var popup = new OrderDetailPopup(viewModel)
            {
                Owner = Application.Current?.MainWindow
            };
            popup.ShowDialog();
        }
    }
}
