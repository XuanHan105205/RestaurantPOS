using System.Windows.Input;
using RestaurantPOS.MVVM;

namespace RestaurantPOS.ViewModels.Core
{
    public class LoginViewModel : ViewModelBase
    {
        private string _username;
        private string _errorMessage;

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand LoginCommand { get; set; }

        public LoginViewModel()
        {
            // TODO: Hàn will implement Login logic here.
        }
    }
}
