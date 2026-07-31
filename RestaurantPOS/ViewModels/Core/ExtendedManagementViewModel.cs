using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Globalization;
using RestaurantPOS.Models;
using RestaurantPOS.MVVM;
using RestaurantPOS.Services;

namespace RestaurantPOS.ViewModels.Core
{
    public class ExtendedManagementViewModel : ViewModelBase
    {
        private readonly AcademicManagementService _service = new();
        public bool IsManager => AuthService.Instance.CurrentUser?.Role == "manager";
        public bool CanManageReservations => IsManager || AuthService.Instance.CurrentUser?.Role == "waiter";
        public bool CanManageStock => IsManager || AuthService.Instance.CurrentUser?.Role == "inventory";
        public bool CanViewInvoices => IsManager || AuthService.Instance.CurrentUser?.Role == "cashier";

        public ObservableCollection<Employee> Employees { get; } = new();
        public ObservableCollection<Category> Categories { get; } = new();
        public ObservableCollection<Dish> Dishes { get; } = new();
        public ObservableCollection<RestaurantTable> Tables { get; } = new();
        public ObservableCollection<Customer> Customers { get; } = new();
        public ObservableCollection<Reservation> Reservations { get; } = new();
        public ObservableCollection<Ingredient> Ingredients { get; } = new();
        public ObservableCollection<StockMovement> StockMovements { get; } = new();
        public ObservableCollection<Invoice> Invoices { get; } = new();
        public ObservableCollection<AuditLog> AuditLogs { get; } = new();
        public string[] Roles { get; } = { "manager", "waiter", "kitchen", "cashier", "inventory" };
        public string[] DishStatuses { get; } = { "active", "discontinued" };

        private Employee? _selectedEmployee;
        public Employee? SelectedEmployee { get => _selectedEmployee; set { if (SetProperty(ref _selectedEmployee, value)) NewPassword = ""; } }
        private string _newPassword = ""; public string NewPassword { get => _newPassword; set => SetProperty(ref _newPassword, value); }
        private Category? _selectedCategory; public Category? SelectedCategory { get => _selectedCategory; set => SetProperty(ref _selectedCategory, value); }
        private Dish? _selectedDish; public Dish? SelectedDish { get => _selectedDish; set => SetProperty(ref _selectedDish, value); }
        private RestaurantTable? _selectedTable; public RestaurantTable? SelectedTable { get => _selectedTable; set => SetProperty(ref _selectedTable, value); }
        private Reservation? _selectedReservation; public Reservation? SelectedReservation { get => _selectedReservation; set { if (SetProperty(ref _selectedReservation, value) && value != null) ReservationTimeText = value.ReservationTime.ToString("dd/MM/yyyy HH:mm"); } }
        private string _reservationTimeText = ""; public string ReservationTimeText { get => _reservationTimeText; set => SetProperty(ref _reservationTimeText, value); }
        private string _quickCustomerName = ""; public string QuickCustomerName { get => _quickCustomerName; set => SetProperty(ref _quickCustomerName, value); }
        private string _quickCustomerPhone = ""; public string QuickCustomerPhone { get => _quickCustomerPhone; set => SetProperty(ref _quickCustomerPhone, value); }
        private Ingredient? _selectedIngredient; public Ingredient? SelectedIngredient { get => _selectedIngredient; set => SetProperty(ref _selectedIngredient, value); }
        private Invoice? _selectedInvoice; public Invoice? SelectedInvoice { get => _selectedInvoice; set => SetProperty(ref _selectedInvoice, value); }
        private decimal _adjustmentQuantity; public decimal AdjustmentQuantity { get => _adjustmentQuantity; set => SetProperty(ref _adjustmentQuantity, value); }
        private string _reason = ""; public string Reason { get => _reason; set => SetProperty(ref _reason, value); }
        private string _statusMessage = ""; public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }

        public ICommand RefreshCommand { get; }
        public ICommand NewEmployeeCommand { get; }
        public ICommand SaveEmployeeCommand { get; }
        public ICommand ToggleEmployeeCommand { get; }
        public ICommand NewCategoryCommand { get; }
        public ICommand SaveCategoryCommand { get; }
        public ICommand NewDishCommand { get; }
        public ICommand SaveDishCommand { get; }
        public ICommand NewTableCommand { get; }
        public ICommand SaveTableCommand { get; }
        public ICommand NewReservationCommand { get; }
        public ICommand SaveReservationCommand { get; }
        public ICommand CreateQuickCustomerCommand { get; }
        public ICommand CheckInCommand { get; }
        public ICommand CancelReservationCommand { get; }
        public ICommand AdjustInCommand { get; }
        public ICommand AdjustOutCommand { get; }
        public ICommand WasteCommand { get; }
        public ICommand CancelInvoiceCommand { get; }
        public ICommand PrintInvoiceCommand { get; }

        public ExtendedManagementViewModel()
        {
            RefreshCommand = new RelayCommand(Load);
            NewEmployeeCommand = new RelayCommand(() => SelectedEmployee = new Employee { IsActive = true, Role = "waiter", CreatedAt = DateTime.Now });
            SaveEmployeeCommand = new RelayCommand(() => Run(() => SelectedEmployee == null ? (false, "Chọn hoặc tạo nhân viên.") : _service.SaveEmployee(SelectedEmployee, NewPassword)));
            ToggleEmployeeCommand = new RelayCommand(() => Run(() => SelectedEmployee == null ? (false, "Chọn nhân viên.") : _service.SetEmployeeActive(SelectedEmployee, !SelectedEmployee.IsActive)));
            NewCategoryCommand = new RelayCommand(() => SelectedCategory = new Category());
            SaveCategoryCommand = new RelayCommand(() => Run(() => SelectedCategory == null ? (false, "Chọn hoặc tạo danh mục.") : _service.SaveCategory(SelectedCategory)));
            NewDishCommand = new RelayCommand(() => SelectedDish = new Dish { AvailabilityStatus = "active" });
            SaveDishCommand = new RelayCommand(() => Run(() => SelectedDish == null ? (false, "Chọn hoặc tạo món.") : _service.SaveDish(SelectedDish)));
            NewTableCommand = new RelayCommand(() => SelectedTable = new RestaurantTable { Capacity = 4, Status = "available", IsActive = true });
            SaveTableCommand = new RelayCommand(() => Run(() => SelectedTable == null ? (false, "Chọn hoặc tạo bàn.") : _service.SaveTable(SelectedTable)));
            NewReservationCommand = new RelayCommand(() => SelectedReservation = new Reservation { ReservationTime = DateTime.Now.AddHours(1), GuestCount = 2, Status = "confirmed" });
            SaveReservationCommand = new RelayCommand(SaveReservation);
            CreateQuickCustomerCommand = new RelayCommand(CreateQuickCustomer);
            CheckInCommand = new RelayCommand(() => Run(() => SelectedReservation == null ? (false, "Chọn đặt bàn.") : _service.SetReservationStatus(SelectedReservation, "checked_in")));
            CancelReservationCommand = new RelayCommand(() => Run(() => SelectedReservation == null ? (false, "Chọn đặt bàn.") : _service.SetReservationStatus(SelectedReservation, "cancelled")));
            AdjustInCommand = new RelayCommand(() => Adjust(true, false)); AdjustOutCommand = new RelayCommand(() => Adjust(false, false)); WasteCommand = new RelayCommand(() => Adjust(false, true));
            CancelInvoiceCommand = new RelayCommand(() => Run(() => SelectedInvoice == null ? (false, "Chọn hóa đơn.") : _service.CancelInvoice(SelectedInvoice, Reason)));
            PrintInvoiceCommand = new RelayCommand(() => { if (SelectedInvoice == null) StatusMessage = "Chọn hóa đơn."; else StatusMessage = new InvoicePrintService().Print(SelectedInvoice, _service.GetInvoiceLines(SelectedInvoice.SessionId)) ? "Đã gửi hóa đơn tới máy in." : "Đã hủy in."; });
            Load();
        }

        private void Adjust(bool increase, bool waste) => Run(() => SelectedIngredient == null ? (false, "Chọn nguyên liệu.") : _service.AdjustStock(SelectedIngredient.IngredientId, AdjustmentQuantity, increase, waste, Reason));
        private void SaveReservation()
        {
            if (SelectedReservation == null) { StatusMessage = "Chọn hoặc tạo đặt bàn."; return; }
            if (!DateTime.TryParseExact(ReservationTimeText, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var time))
            { StatusMessage = "Thời gian phải theo dạng dd/MM/yyyy HH:mm."; return; }
            SelectedReservation.ReservationTime = time;
            Run(() => _service.SaveReservation(SelectedReservation));
        }
        private void CreateQuickCustomer()
        {
            var customer = new Customer { FullName = QuickCustomerName, Phone = QuickCustomerPhone, MembershipTier = "regular", LoyaltyPoints = 0 };
            var customerService = new CustomerService();
            if (!customerService.AddCustomer(customer)) { StatusMessage = "Không thể tạo khách: kiểm tra tên, SĐT hoặc SĐT đã tồn tại."; return; }
            Replace(Customers, _service.GetCustomers());
            if (SelectedReservation == null) SelectedReservation = new Reservation { ReservationTime = DateTime.Now.AddHours(1), GuestCount = 2, Status = "confirmed" };
            SelectedReservation.CustomerId = customer.CustomerId;
            QuickCustomerName = QuickCustomerPhone = ""; StatusMessage = "Đã tạo và chọn khách hàng mới.";
        }
        private void Run(Func<(bool Success, string Message)> operation) { try { var result = operation(); StatusMessage = result.Message; if (result.Success) Load(); } catch (Exception ex) { StatusMessage = "Lỗi: " + ex.Message; } }
        private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> values) { target.Clear(); foreach (var value in values) target.Add(value); }
        private void Load()
        {
            try
            {
                if (IsManager) { Replace(Employees, _service.GetEmployees()); Replace(Categories, _service.GetCategories()); Replace(Dishes, _service.GetDishes()); Replace(AuditLogs, _service.GetAuditLogs()); }
                Replace(Tables, _service.GetTables()); Replace(Customers, _service.GetCustomers());
                if (CanManageReservations) Replace(Reservations, _service.GetReservations());
                if (CanManageStock) { Replace(Ingredients, new IngredientService().GetAllIngredients()); Replace(StockMovements, _service.GetStockMovements()); }
                if (CanViewInvoices) Replace(Invoices, _service.GetInvoices());
            }
            catch (Exception ex) { StatusMessage = "Chưa nâng cấp DB hoặc lỗi kết nối: " + ex.Message; }
        }
    }
}
