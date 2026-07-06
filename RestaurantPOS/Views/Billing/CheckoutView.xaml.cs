using System.Windows;
using System.Windows.Controls;
using RestaurantPOS.Models;
using RestaurantPOS.ViewModels.Billing;

namespace RestaurantPOS.Views.Billing
{
    public partial class CheckoutView : UserControl
    {
        public CheckoutView()
        {
            InitializeComponent();
        }

        private void TableCard_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is RestaurantTable table)
            {
                if (DataContext is CheckoutViewModel vm)
                {
                    vm.SelectedTable = table;
                }
            }
        }
    }
}
