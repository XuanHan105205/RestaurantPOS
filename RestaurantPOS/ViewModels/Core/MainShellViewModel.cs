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
        private readonly IInventoryViewModelFactory _inventoryFactory;

        public MainShellViewModel(
            IAuthService authService,
            IWindowNavigationService windowNavigation,
            IDialogService dialogService,
            IInventoryViewModelFactory inventoryFactory)
        {
            _authService = authService;
            _windowNavigation = windowNavigation;
            _dialogService = dialogService;
            _inventoryFactory = inventoryFactory;

            NavigateWaiterCommand = new RelayCommand(NavigateToWaiter);
            NavigateKitchenCommand = new RelayCommand(NavigateToKitchen);
            NavigateInventoryCommand = new RelayCommand(NavigateToInventory);
            NavigateBillingCommand = new RelayCommand(NavigateToBilling);
            NavigateCustomerCommand = new RelayCommand(NavigateToCustomer);
            NavigateExtendedCommand = new RelayCommand(
                () => Navigation.CurrentViewModel = new ExtendedManagementViewModel());
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
        public bool IsInventoryVisible => CurrentEmployeeRole == "MANAGER";
        public bool IsBillingVisible =>
            CurrentEmployeeRole == "MANAGER" || CurrentEmployeeRole == "CASHIER";
        public bool IsCustomerVisible => CurrentEmployeeRole == "MANAGER";
        public bool IsExtendedVisible => CurrentEmployeeRole == "MANAGER";

        public ICommand NavigateWaiterCommand { get; }
        public ICommand NavigateKitchenCommand { get; }
        public ICommand NavigateInventoryCommand { get; }
        public ICommand NavigateBillingCommand { get; }
        public ICommand NavigateCustomerCommand { get; }
        public ICommand NavigateExtendedCommand { get; }
        public ICommand LogoutCommand { get; }

        private CustomerManagementViewModel CreateCustomerViewModel()
        {
            return new CustomerManagementViewModel(
                new CustomerService(),
                _dialogService);
        }

        private void NavigateToWaiter()
        {
            Navigation.CurrentViewModel = new TableViewModel();
        }

        private void NavigateToKitchen()
        {
            Navigation.CurrentViewModel = new KitchenViewModel();
        }

        private void NavigateToInventory()
        {
            Navigation.CurrentViewModel = _inventoryFactory.Create();
        }

        private void NavigateToBilling()
        {
            Navigation.CurrentViewModel = new BillingViewModel();
        }

        private void NavigateToCustomer()
        {
            Navigation.CurrentViewModel = CreateCustomerViewModel();
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
