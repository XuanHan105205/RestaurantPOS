using System;
using System.Windows.Input;
using RestaurantPOS.MVVM;
using RestaurantPOS.Services;

namespace RestaurantPOS.ViewModels.Core
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly IWindowNavigationService _windowNavigation;
        private string _username = string.Empty;
        private string _errorMessage = string.Empty;

        public LoginViewModel(
            IAuthService authService,
            IWindowNavigationService windowNavigation)
        {
            _authService = authService;
            _windowNavigation = windowNavigation;
            LoginCommand = new RelayCommand<IPasswordProvider>(
                ExecuteLogin,
                CanExecuteLogin);
        }

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (SetProperty(ref _errorMessage, value))
                {
                    OnPropertyChanged(nameof(HasError));
                }
            }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
        public ICommand LoginCommand { get; }

        private bool CanExecuteLogin(IPasswordProvider passwordProvider)
        {
            return !string.IsNullOrWhiteSpace(Username);
        }

        private void ExecuteLogin(IPasswordProvider passwordProvider)
        {
            if (passwordProvider == null)
            {
                return;
            }

            try
            {
                if (_authService.Login(Username, passwordProvider.Password))
                {
                    ErrorMessage = string.Empty;
                    _windowNavigation.OpenMainShell();
                    return;
                }

                ErrorMessage = "Tên đăng nhập hoặc mật khẩu không đúng!";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Lỗi kết nối DB: {ex.Message}";
            }
        }
    }
}
