using System.Windows;
using RestaurantPOS.ViewModels.Core;
using RestaurantPOS.Views.Core;

namespace RestaurantPOS.Services
{
    public class WpfWindowNavigationService : IWindowNavigationService
    {
        private readonly IAuthService _authService;
        private readonly IDialogService _dialogService;
        private readonly IInventoryViewModelFactory _inventoryFactory;

        public WpfWindowNavigationService(
            IAuthService authService,
            IDialogService dialogService,
            IInventoryViewModelFactory inventoryFactory)
        {
            _authService = authService;
            _dialogService = dialogService;
            _inventoryFactory = inventoryFactory;
        }

        public void OpenLogin()
        {
            var viewModel = new LoginViewModel(_authService, this);
            SwitchMainWindow(new LoginWindow(viewModel));
        }

        public void OpenMainShell()
        {
            var viewModel = new MainShellViewModel(
                _authService,
                this,
                _dialogService,
                _inventoryFactory);
            SwitchMainWindow(new MainShellWindow(viewModel));
        }

        private static void SwitchMainWindow(Window nextWindow)
        {
            var application = Application.Current;
            var previousWindow = application?.MainWindow;

            if (application != null)
            {
                application.MainWindow = nextWindow;
            }

            nextWindow.Show();

            if (previousWindow != null && previousWindow != nextWindow)
            {
                previousWindow.Close();
            }
        }
    }
}
