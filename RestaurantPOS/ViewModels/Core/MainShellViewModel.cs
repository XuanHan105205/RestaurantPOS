using System.Windows.Input;
using RestaurantPOS.MVVM;

namespace RestaurantPOS.ViewModels.Core
{
    public class MainShellViewModel : ViewModelBase
    {
        public string CurrentEmployeeName { get; set; } = "Hàn";
        public string CurrentEmployeeRole { get; set; } = "MANAGER";

        public NavigationService Navigation => NavigationService.Instance;

        public ICommand NavigateWaiterCommand { get; set; }
        public ICommand NavigateKitchenCommand { get; set; }
        public ICommand NavigateInventoryCommand { get; set; }
        public ICommand NavigateBillingCommand { get; set; }
        public ICommand NavigateCustomerCommand { get; set; }
        public ICommand LogoutCommand { get; set; }

        public MainShellViewModel()
        {
            // TODO: Hàn will initialize navigation commands and role-based visibility.
        }
    }
}
