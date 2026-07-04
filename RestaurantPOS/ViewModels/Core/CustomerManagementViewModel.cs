using System.Collections.ObjectModel;
using System.Windows.Input;
using RestaurantPOS.MVVM;
using RestaurantPOS.Models;
using RestaurantPOS.Services;

namespace RestaurantPOS.ViewModels.Core
{
    public class CustomerManagementViewModel : ViewModelBase
    {
        private readonly ICustomerService _customerService;

        // Danh sách khách hàng hiển thị trên DataGrid
        private ObservableCollection<Customer> _customers;
        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            set => SetProperty(ref _customers, value);
        }

        // Khách hàng đang được chọn trên DataGrid
        private Customer _selectedCustomer;
        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                {
                    LoadSelectedCustomerToForm();
                }
            }
        }

        // Các thuộc tính của Form nhập liệu
        private string _fullName = string.Empty;
        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        private string _phone = string.Empty;
        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        private string _membershipTier = "regular";
        public string MembershipTier
        {
            get => _membershipTier;
            set => SetProperty(ref _membershipTier, value);
        }

        private int _loyaltyPoints;
        public int LoyaltyPoints
        {
            get => _loyaltyPoints;
            set => SetProperty(ref _loyaltyPoints, value);
        }

        // Từ khóa tìm kiếm
        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        // Trạng thái thông báo
        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        // Các Command
        public ICommand LoadCustomersCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand AddCustomerCommand { get; }
        public ICommand UpdateCustomerCommand { get; }
        public ICommand DeleteCustomerCommand { get; }
        public ICommand ClearFormCommand { get; }

        public CustomerManagementViewModel()
        {
            _customerService = new CustomerService();
            Customers = new ObservableCollection<Customer>();

            // Khởi tạo các Command
            LoadCustomersCommand = new RelayCommand(ExecuteLoadCustomers);
            SearchCommand = new RelayCommand(ExecuteSearch);
            AddCustomerCommand = new RelayCommand(ExecuteAddCustomer, CanExecuteAddCustomer);
            UpdateCustomerCommand = new RelayCommand(ExecuteUpdateCustomer, CanExecuteUpdateOrDelete);
            DeleteCustomerCommand = new RelayCommand(ExecuteDeleteCustomer, CanExecuteUpdateOrDelete);
            ClearFormCommand = new RelayCommand(ExecuteClearForm);

            // Tải dữ liệu ban đầu
            ExecuteLoadCustomers();
        }

        private void ExecuteLoadCustomers()
        {
            var list = _customerService.GetAllCustomers();
            Customers = new ObservableCollection<Customer>(list);
            StatusMessage = "Đã tải danh sách khách hàng.";
        }

        private void ExecuteSearch()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                ExecuteLoadCustomers();
                return;
            }

            var customer = _customerService.GetCustomerByPhone(SearchText.Trim());
            Customers.Clear();
            if (customer != null)
            {
                Customers.Add(customer);
                StatusMessage = $"Tìm thấy khách hàng: {customer.FullName}";
            }
            else
            {
                StatusMessage = "Không tìm thấy khách hàng nào với số điện thoại này.";
            }
        }

        private bool CanExecuteAddCustomer()
        {
            return !string.IsNullOrWhiteSpace(FullName) && !string.IsNullOrWhiteSpace(Phone);
        }

        private void ExecuteAddCustomer()
        {
            var newCustomer = new Customer
            {
                FullName = FullName.Trim(),
                Phone = Phone.Trim(),
                MembershipTier = MembershipTier,
                LoyaltyPoints = LoyaltyPoints
            };

            bool success = _customerService.AddCustomer(newCustomer);
            if (success)
            {
                ExecuteLoadCustomers();
                ExecuteClearForm();
                StatusMessage = "Thêm khách hàng thành công!";
            }
            else
            {
                StatusMessage = "Thêm thất bại! Số điện thoại có thể đã tồn tại.";
            }
        }

        private bool CanExecuteUpdateOrDelete()
        {
            return SelectedCustomer != null;
        }

        private void ExecuteUpdateCustomer()
        {
            if (SelectedCustomer == null) return;

            SelectedCustomer.FullName = FullName.Trim();
            SelectedCustomer.Phone = Phone.Trim();
            SelectedCustomer.MembershipTier = MembershipTier;
            SelectedCustomer.LoyaltyPoints = LoyaltyPoints;

            bool success = _customerService.UpdateCustomer(SelectedCustomer);
            if (success)
            {
                ExecuteLoadCustomers();
                ExecuteClearForm();
                StatusMessage = "Cập nhật thông tin khách hàng thành công!";
            }
            else
            {
                StatusMessage = "Cập nhật thông tin thất bại.";
            }
        }

        private void ExecuteDeleteCustomer()
        {
            if (SelectedCustomer == null) return;

            var result = System.Windows.MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa khách hàng '{SelectedCustomer.FullName}' không?", 
                "Xác nhận xóa", 
                System.Windows.MessageBoxButton.YesNo, 
                System.Windows.MessageBoxImage.Warning);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                bool success = _customerService.DeleteCustomer(SelectedCustomer.CustomerId);
                if (success)
                {
                    ExecuteLoadCustomers();
                    ExecuteClearForm();
                    StatusMessage = "Đã xóa khách hàng thành công!";
                }
                else
                {
                    StatusMessage = "Xóa khách hàng thất bại.";
                }
            }
        }

        private void LoadSelectedCustomerToForm()
        {
            if (SelectedCustomer != null)
            {
                FullName = SelectedCustomer.FullName;
                Phone = SelectedCustomer.Phone;
                MembershipTier = SelectedCustomer.MembershipTier;
                LoyaltyPoints = SelectedCustomer.LoyaltyPoints;
            }
        }

        private void ExecuteClearForm()
        {
            SelectedCustomer = null;
            FullName = string.Empty;
            Phone = string.Empty;
            MembershipTier = "regular";
            LoyaltyPoints = 0;
            StatusMessage = "Đã làm trống Form nhập liệu.";
        }
    }
}
