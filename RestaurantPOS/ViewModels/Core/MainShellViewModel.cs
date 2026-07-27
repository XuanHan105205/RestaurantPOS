using System.Windows.Input;
using RestaurantPOS.MVVM;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Billing;
using RestaurantPOS.ViewModels.Inventory;
using RestaurantPOS.ViewModels.Kitchen;
using RestaurantPOS.ViewModels.Waiter;

namespace RestaurantPOS.ViewModels.Core
{
    public class MainShellViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;
        private readonly IWindowNavigationService _windowNavigation;
        private readonly IDialogService _dialogService;

        public MainShellViewModel(
            IAuthService authService,
            IWindowNavigationService windowNavigation,
            IDialogService dialogService)
        {
            _authService = authService;
            _windowNavigation = windowNavigation;
            _dialogService = dialogService;

            NavigateWaiterCommand = new RelayCommand(
                () => Navigation.CurrentViewModel = new TableViewModel());
            NavigateKitchenCommand = new RelayCommand(
                () => Navigation.CurrentViewModel = new KitchenViewModel());
            NavigateInventoryCommand = new RelayCommand(
                () => Navigation.CurrentViewModel = new InventoryViewModel());
            NavigateBillingCommand = new RelayCommand(
                () => Navigation.CurrentViewModel = new BillingViewModel());
            NavigateCustomerCommand = new RelayCommand(
                () => Navigation.CurrentViewModel = CreateCustomerViewModel());
            LogoutCommand = new RelayCommand(ExecuteLogout);

            SetDefaultView();
        }

        public NavigationService Navigation => NavigationService.Instance;
        public string CurrentEmployeeName =>
            _authService.CurrentUser?.FullName ?? "Chưa đăng nhập";
        public string CurrentEmployeeRole =>
            _authService.CurrentUser?.Role?.ToUpper() ?? "UNKNOWN";

        public bool IsWaiterVisible =>
            CurrentEmployeeRole == "MANAGER" || CurrentEmployeeRole == "WAITER";
        public bool IsKitchenVisible =>
            CurrentEmployeeRole == "MANAGER" || CurrentEmployeeRole == "KITCHEN";
        public bool IsInventoryVisible =>
            CurrentEmployeeRole == "MANAGER" || CurrentEmployeeRole == "INVENTORY";
        public bool IsBillingVisible =>
            CurrentEmployeeRole == "MANAGER" || CurrentEmployeeRole == "CASHIER";
        public bool IsCustomerVisible => CurrentEmployeeRole == "MANAGER";

        public ICommand NavigateWaiterCommand { get; }
        public ICommand NavigateKitchenCommand { get; }
        public ICommand NavigateInventoryCommand { get; }
        public ICommand NavigateBillingCommand { get; }
        public ICommand NavigateCustomerCommand { get; }
        public ICommand LogoutCommand { get; }

        private CustomerManagementViewModel CreateCustomerViewModel()
        {
            return new CustomerManagementViewModel(
                new CustomerService(),
                _dialogService);
        }

        private void SetDefaultView()
        {
            switch (_authService.CurrentUser?.Role?.ToLower())
            {
                case "waiter":
                    Navigation.CurrentViewModel = new TableViewModel();
                    break;
                case "kitchen":
                    Navigation.CurrentViewModel = new KitchenViewModel();
                    break;
                case "inventory":
                    Navigation.CurrentViewModel = new InventoryViewModel();
                    break;
                case "cashier":
                    Navigation.CurrentViewModel = new BillingViewModel();
                    break;
                case "manager":
                default:
                    Navigation.CurrentViewModel = CreateCustomerViewModel();
                    break;
            }
        }

        private void ExecuteLogout()
        {
            _authService.Logout();
            _windowNavigation.OpenLogin();
        }
    }
}
