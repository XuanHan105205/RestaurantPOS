using System.Windows;
using System.Windows.Input;
using RestaurantPOS.MVVM;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Waiter;
using RestaurantPOS.ViewModels.Kitchen;
using RestaurantPOS.ViewModels.Inventory;
using RestaurantPOS.ViewModels.Billing;

namespace RestaurantPOS.ViewModels.Core
{
    public class MainShellViewModel : ViewModelBase
    {
        public NavigationService Navigation => NavigationService.Instance;

        // Lấy thông tin nhân viên đăng nhập hiện tại từ AuthService
        public string CurrentEmployeeName => AuthService.Instance.CurrentUser?.FullName ?? "Chưa đăng nhập";
        public string CurrentEmployeeRole => AuthService.Instance.CurrentUser?.Role?.ToUpper() ?? "UNKNOWN";

        // Phân quyền hiển thị các Tab trên Sidebar dựa trên vai trò (Role)
        public bool IsWaiterVisible => CurrentEmployeeRole == "MANAGER" || CurrentEmployeeRole == "WAITER";
        public bool IsKitchenVisible => CurrentEmployeeRole == "MANAGER" || CurrentEmployeeRole == "KITCHEN";
        public bool IsInventoryVisible => CurrentEmployeeRole == "MANAGER" || CurrentEmployeeRole == "INVENTORY";
        public bool IsBillingVisible => CurrentEmployeeRole == "MANAGER" || CurrentEmployeeRole == "CASHIER";
        public bool IsCustomerVisible => CurrentEmployeeRole == "MANAGER"; // Hàn (Manager) quản lý khách hàng

        // Khai báo các Command chuyển trang và Đăng xuất
        public ICommand NavigateWaiterCommand { get; }
        public ICommand NavigateKitchenCommand { get; }
        public ICommand NavigateInventoryCommand { get; }
        public ICommand NavigateBillingCommand { get; }
        public ICommand NavigateCustomerCommand { get; }
        public ICommand LogoutCommand { get; }

        public MainShellViewModel()
        {
            // Khởi tạo các Command điều hướng
            NavigateWaiterCommand = new RelayCommand(() => Navigation.CurrentViewModel = new TableViewModel());
            NavigateKitchenCommand = new RelayCommand(() => Navigation.CurrentViewModel = new KitchenViewModel());
            NavigateInventoryCommand = new RelayCommand(() => Navigation.CurrentViewModel = new InventoryViewModel());
            NavigateBillingCommand = new RelayCommand(() => Navigation.CurrentViewModel = new BillingViewModel());
            NavigateCustomerCommand = new RelayCommand(() => Navigation.CurrentViewModel = new CustomerManagementViewModel());

            // Command Đăng xuất (nhận đối tượng Window hiện tại làm tham số để đóng cửa sổ)
            LogoutCommand = new RelayCommand<Window>(ExecuteLogout);

            // Đặt View mặc định hiển thị ban đầu dựa trên Role của nhân viên
            SetDefaultView();
        }

        private void SetDefaultView()
        {
            string role = AuthService.Instance.CurrentUser?.Role?.ToLower();
            switch (role)
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
                    Navigation.CurrentViewModel = new CustomerManagementViewModel();
                    break;
            }
        }

        private void ExecuteLogout(Window currentWindow)
        {
            AuthService.Instance.Logout();
            
            // Mở lại màn hình Đăng nhập
            var loginWindow = new Views.Core.LoginWindow();
            loginWindow.Show();

            // Đóng màn hình chính hiện tại
            currentWindow?.Close();
        }
    }
}
