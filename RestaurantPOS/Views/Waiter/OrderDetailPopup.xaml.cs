using System.Windows;
using RestaurantPOS.ViewModels.Waiter;

namespace RestaurantPOS.Views.Waiter
{
    public partial class OrderDetailPopup : Window
    {
        public OrderDetailPopup(OrderDetailPopupViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.RequestClose += (result) =>
            {
                DialogResult = result;
                Close();
            };
        }
    }
}
