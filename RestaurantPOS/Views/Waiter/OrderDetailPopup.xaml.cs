using System.Windows;
using RestaurantPOS.Models;
using RestaurantPOS.ViewModels.Waiter;

namespace RestaurantPOS.Views.Waiter
{
    public partial class OrderDetailPopup : Window
    {
        public OrderDetailPopup(DiningSession session, string tableName)
        {
            InitializeComponent();
            DataContext = new OrderDetailPopupViewModel(session, tableName);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
