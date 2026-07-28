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
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
