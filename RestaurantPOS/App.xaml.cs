using System.Windows;
using RestaurantPOS.Services;

namespace RestaurantPOS;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        IAuthService authService = AuthService.Instance;
        IDialogService dialogService = new WpfDialogService();
        IInventoryViewModelFactory inventoryFactory = new InventoryViewModelFactory(dialogService);
        IWindowNavigationService windowNavigation =
            new WpfWindowNavigationService(authService, dialogService, inventoryFactory);

        windowNavigation.OpenLogin();
    }
}
