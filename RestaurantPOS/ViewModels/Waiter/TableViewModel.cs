using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using RestaurantPOS.Models;
using RestaurantPOS.MVVM;
using RestaurantPOS.Services;

namespace RestaurantPOS.ViewModels.Waiter
{
    public class TableViewModel : ViewModelBase
    {
        private readonly ITableService _tableService;
        private readonly ICustomerService _customerService;
        private readonly IDialogService _dialogService;
        private readonly IOrderDetailDialogService _orderDetailDialogService;

        private ObservableCollection<RestaurantTable> _tables = new();
        public ObservableCollection<RestaurantTable> Tables
        {
            get => _tables;
            set => SetProperty(ref _tables, value);
        }

        private RestaurantTable? _selectedTable;
        public RestaurantTable? SelectedTable
        {
            get => _selectedTable;
            set
            {
                if (SetProperty(ref _selectedTable, value))
                {
                    OnTableSelected();
                    OnPropertyChanged(nameof(IsTableSelected));
                    OnPropertyChanged(nameof(IsNoTableSelected));
                }
            }
        }

        public bool IsTableSelected => SelectedTable != null;
        public bool IsNoTableSelected => SelectedTable == null;

        private DiningSession? _activeSession;
        public DiningSession? ActiveSession
        {
            get => _activeSession;
            set => SetProperty(ref _activeSession, value);
        }

        private Customer? _selectedCustomer;
        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set => SetProperty(ref _selectedCustomer, value);
        }

        private string _customerPhoneSearch = "";
        public string CustomerPhoneSearch
        {
            get => _customerPhoneSearch;
            set
            {
                value ??= "";
                if (SetProperty(ref _customerPhoneSearch, value))
                {
                    if (SelectedCustomer?.Phone != value.Trim())
                    {
                        SelectedCustomer = null;
                    }
                    IsQuickRegistrationVisible = false;
                    QuickCustomerName = "";
                    CustomerSearchResultMessage = "";
                }
            }
        }

        private string _quickCustomerName = "";
        public string QuickCustomerName
        {
            get => _quickCustomerName;
            set => SetProperty(ref _quickCustomerName, value);
        }

        private bool _isQuickRegistrationVisible;
        public bool IsQuickRegistrationVisible
        {
            get => _isQuickRegistrationVisible;
            set => SetProperty(ref _isQuickRegistrationVisible, value);
        }

        private string _customerSearchResultMessage = "";
        public string CustomerSearchResultMessage
        {
            get => _customerSearchResultMessage;
            set => SetProperty(ref _customerSearchResultMessage, value);
        }

        private bool _isSessionInfoVisible;
        public bool IsSessionInfoVisible
        {
            get => _isSessionInfoVisible;
            set => SetProperty(ref _isSessionInfoVisible, value);
        }

        private bool _isCleaningAllowed;
        public bool IsCleaningAllowed
        {
            get => _isCleaningAllowed;
            set => SetProperty(ref _isCleaningAllowed, value);
        }

        private bool _isOpenSessionAllowed;
        public bool IsOpenSessionAllowed
        {
            get => _isOpenSessionAllowed;
            set => SetProperty(ref _isOpenSessionAllowed, value);
        }

        private bool _isReserved;
        public bool IsReserved
        {
            get => _isReserved;
            set => SetProperty(ref _isReserved, value);
        }

        public ICommand LoadTablesCommand { get; }
        public ICommand SearchCustomerCommand { get; }
        public ICommand RegisterCustomerCommand { get; }
        public ICommand OpenSessionCommand { get; }
        public ICommand CleanTableCommand { get; }
        public ICommand GoToOrderCommand { get; }
        public ICommand ViewOrderDetailsCommand { get; }

        public TableViewModel()
            : this(
                new TableService(),
                new CustomerService(),
                new WpfDialogService(),
                new WpfOrderDetailDialogService(new WpfDialogService()))
        {
        }

        public TableViewModel(
            ITableService tableService,
            ICustomerService customerService,
            IDialogService dialogService,
            IOrderDetailDialogService orderDetailDialogService)
        {
            _tableService = tableService;
            _customerService = customerService;
            _dialogService = dialogService;
            _orderDetailDialogService = orderDetailDialogService;

            LoadTablesCommand = new RelayCommand(LoadTables);
            SearchCustomerCommand = new RelayCommand(SearchCustomer);
            RegisterCustomerCommand = new RelayCommand(RegisterCustomer);
            OpenSessionCommand = new RelayCommand(OpenSession);
            CleanTableCommand = new RelayCommand(CleanTable);
            GoToOrderCommand = new RelayCommand(GoToOrder);
            ViewOrderDetailsCommand = new RelayCommand(ViewOrderDetails);

            LoadTables();
        }

        private void LoadTables()
        {
            var list = _tableService.GetAllTables();
            Tables = new ObservableCollection<RestaurantTable>(list);
            
            // Re-select table if it was selected before
            if (SelectedTable != null)
            {
                SelectedTable = Tables.FirstOrDefault(t => t.TableId == SelectedTable.TableId);
            }
        }

        private void OnTableSelected()
        {
            if (SelectedTable == null)
            {
                ActiveSession = null;
                SelectedCustomer = null;
                IsSessionInfoVisible = false;
                IsOpenSessionAllowed = false;
                IsCleaningAllowed = false;
                IsReserved = false;
                return;
            }

            IsCleaningAllowed = SelectedTable.Status == "needs_cleaning";
            IsOpenSessionAllowed = SelectedTable.Status == "available";
            IsReserved = SelectedTable.Status == "reserved";

            if (SelectedTable.Status == "occupied")
            {
                ActiveSession = _tableService.GetActiveSessionByTableId(SelectedTable.TableId);
                if (ActiveSession != null)
                {
                    IsSessionInfoVisible = true;
                    if (ActiveSession.CustomerId.HasValue)
                    {
                        var custs = _customerService.GetAllCustomers();
                        SelectedCustomer = custs.FirstOrDefault(c => c.CustomerId == ActiveSession.CustomerId.Value);
                    }
                    else
                    {
                        SelectedCustomer = null;
                    }
                }
                else
                {
                    IsSessionInfoVisible = false;
                    SelectedCustomer = null;
                }
            }
            else
            {
                ActiveSession = null;
                SelectedCustomer = null;
                IsSessionInfoVisible = false;
                CustomerPhoneSearch = "";
                CustomerSearchResultMessage = "";
                QuickCustomerName = "";
                IsQuickRegistrationVisible = false;
            }
        }

        private void SearchCustomer()
        {
            if (string.IsNullOrWhiteSpace(CustomerPhoneSearch))
            {
                CustomerSearchResultMessage = "Vui lòng nhập SĐT";
                IsQuickRegistrationVisible = false;
                return;
            }

            var customer = _customerService.GetCustomerByPhone(CustomerPhoneSearch);
            if (customer != null)
            {
                SelectedCustomer = customer;
                CustomerSearchResultMessage = $"Tìm thấy: {customer.FullName} ({customer.MembershipTier})";
                IsQuickRegistrationVisible = false;
            }
            else
            {
                SelectedCustomer = null;
                CustomerSearchResultMessage = "Chưa có khách hàng này. Nhập tên để đăng ký tích điểm hoặc bỏ trống để mở bàn vãng lai.";
                IsQuickRegistrationVisible = true;
            }
        }

        private void RegisterCustomer()
        {
            string phone = CustomerPhoneSearch.Trim();
            string fullName = QuickCustomerName.Trim();
            if (string.IsNullOrWhiteSpace(fullName))
            {
                CustomerSearchResultMessage = "Vui lòng nhập tên khách hàng.";
                return;
            }

            var customer = new Customer
            {
                FullName = fullName,
                Phone = phone,
                MembershipTier = "regular",
                LoyaltyPoints = 0
            };

            if (!_customerService.AddCustomer(customer))
            {
                CustomerSearchResultMessage = "Không thể đăng ký. SĐT phải gồm 9–15 chữ số và chưa được sử dụng.";
                return;
            }

            SelectedCustomer = _customerService.GetCustomerByPhone(phone) ?? customer;
            IsQuickRegistrationVisible = false;
            CustomerSearchResultMessage = $"Đã đăng ký và chọn khách: {SelectedCustomer.FullName}.";
        }

        private void OpenSession()
        {
            if (SelectedTable == null || SelectedTable.Status != "available") return;

            int? employeeId = AuthService.Instance.CurrentUser?.EmployeeId;
            if (!employeeId.HasValue)
            {
                _dialogService.ShowMessage("Mở bàn", "Vui lòng đăng nhập trước khi mở bàn.");
                return;
            }

            int? customerId = null;
            if (SelectedCustomer != null)
            {
                customerId = SelectedCustomer.CustomerId;
            }
            else
            {
                var walkingCust = _customerService.GetAllCustomers().FirstOrDefault(c => c.Phone == "0000000000" || c.FullName.Contains("vãng lai"));
                if (walkingCust != null)
                {
                    customerId = walkingCust.CustomerId;
                }
            }

            try
            {
                ActiveSession = _tableService.OpenSessionForTable(SelectedTable.TableId, employeeId.Value, customerId);
                LoadTables();
                OnTableSelected();
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage("Mở bàn", $"Lỗi khi mở bàn: {ex.Message}");
            }
        }

        private void CleanTable()
        {
            if (SelectedTable == null || SelectedTable.Status != "needs_cleaning") return;

            if (_tableService.UpdateTableStatus(SelectedTable.TableId, "available"))
            {
                LoadTables();
                OnTableSelected();
            }
        }

        private void GoToOrder()
        {
            if (SelectedTable == null || ActiveSession == null) return;
            NavigationService.Instance.CurrentViewModel = new OrderViewModel(
                SelectedTable,
                ActiveSession,
                new OrderService(),
                _dialogService);
        }

        private void ViewOrderDetails()
        {
            if (SelectedTable == null || ActiveSession == null) return;

            _orderDetailDialogService.Show(ActiveSession, SelectedTable.TableName);
            
            // Refresh tables to display any updates in status or items
            LoadTables();
            OnTableSelected();
        }
    }
}
