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
            set => SetProperty(ref _customerPhoneSearch, value);
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

        public ICommand LoadTablesCommand { get; }
        public ICommand SearchCustomerCommand { get; }
        public ICommand OpenSessionCommand { get; }
        public ICommand CleanTableCommand { get; }
        public ICommand GoToOrderCommand { get; }
        public ICommand ViewOrderDetailsCommand { get; }

        public TableViewModel()
        {
            _tableService = new TableService();
            _customerService = new CustomerService();

            LoadTablesCommand = new RelayCommand(LoadTables);
            SearchCustomerCommand = new RelayCommand(SearchCustomer);
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
                return;
            }

            IsCleaningAllowed = SelectedTable.Status == "needs_cleaning";
            IsOpenSessionAllowed = SelectedTable.Status == "available";

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
            }
        }

        private void SearchCustomer()
        {
            if (string.IsNullOrWhiteSpace(CustomerPhoneSearch))
            {
                CustomerSearchResultMessage = "Vui lòng nhập SĐT";
                return;
            }

            var customer = _customerService.GetCustomerByPhone(CustomerPhoneSearch);
            if (customer != null)
            {
                SelectedCustomer = customer;
                CustomerSearchResultMessage = $"Tìm thấy: {customer.FullName} ({customer.MembershipTier})";
            }
            else
            {
                SelectedCustomer = null;
                CustomerSearchResultMessage = "Không tìm thấy khách hàng. Bàn sẽ mở dưới dạng Khách vãng lai.";
            }
        }

        private void OpenSession()
        {
            if (SelectedTable == null || SelectedTable.Status != "available") return;

            int employeeId = AuthService.Instance.CurrentUser?.EmployeeId ?? 2; // Default to Trần Văn Hưng if not authenticated

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
                ActiveSession = _tableService.OpenSessionForTable(SelectedTable.TableId, employeeId, customerId);
                LoadTables();
                OnTableSelected();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi mở bàn: {ex.Message}");
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
            NavigationService.Instance.CurrentViewModel = new OrderViewModel(SelectedTable, ActiveSession);
        }

        private void ViewOrderDetails()
        {
            if (SelectedTable == null || ActiveSession == null) return;

            var popup = new Views.Waiter.OrderDetailPopup(ActiveSession, SelectedTable.TableName);
            popup.Owner = System.Windows.Application.Current.MainWindow;
            popup.ShowDialog();
            
            // Refresh tables to display any updates in status or items
            LoadTables();
            OnTableSelected();
        }
    }
}
