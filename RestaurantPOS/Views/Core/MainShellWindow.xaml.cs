using System.Windows;
using RestaurantPOS.ViewModels.Core;

namespace RestaurantPOS.Views.Core
{
    public partial class MainShellWindow : Window
    {
        public MainShellWindow(MainShellViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
