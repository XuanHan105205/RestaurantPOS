using System.Windows;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Core;

namespace RestaurantPOS.Views.Core
{
    public partial class LoginWindow : Window, IPasswordProvider
    {
        public LoginWindow(LoginViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        public string Password => PasswordInput.Password;
    }
}
