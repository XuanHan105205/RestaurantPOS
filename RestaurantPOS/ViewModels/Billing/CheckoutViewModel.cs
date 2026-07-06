using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using RestaurantPOS.Models;
using RestaurantPOS.MVVM;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Waiter;

namespace RestaurantPOS.ViewModels.Billing
{
    public class CheckoutViewModel : ViewModelBase
    {
        private readonly ITableService _tableService;
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly IInvoiceService _invoiceService;

        // List of all tables for selection
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
                }
            }
        }

        // Active Session for the selected table
        private DiningSession? _activeSession;
        public DiningSession? ActiveSession
        {
            get => _activeSession;
            set
            {
                if (SetProperty(ref _activeSession, value))
                {
                    OnPropertyChanged(nameof(HasActiveSession));
                }
            }
        }

        public bool HasActiveSession => ActiveSession != null;

        // Items in the current session
        private ObservableCollection<OrderedItemViewModel> _sessionItems = new();
        public ObservableCollection<OrderedItemViewModel> SessionItems
        {
            get => _sessionItems;
            set => SetProperty(ref _sessionItems, value);
        }

        // Selected Customer (if any)
        private Customer? _selectedCustomer;
        public Customer? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                {
                    OnPropertyChanged(nameof(HasSelectedCustomer));
                    OnPropertyChanged(nameof(CustomerDisplayName));
                    RecalculateBill();
                }
            }
        }

        public bool HasSelectedCustomer => SelectedCustomer != null;

        public string CustomerDisplayName => SelectedCustomer != null 
            ? $"{SelectedCustomer.FullName} ({SelectedCustomer.MembershipTier.ToUpper()})" 
            : "Khách vãng lai";

        // Phone search for customer linking
        private string _customerPhoneSearch = string.Empty;
        public string CustomerPhoneSearch
        {
            get => _customerPhoneSearch;
            set => SetProperty(ref _customerPhoneSearch, value);
        }

        private string _customerSearchMessage = string.Empty;
        public string CustomerSearchMessage
        {
            get => _customerSearchMessage;
            set => SetProperty(ref _customerSearchMessage, value);
        }

        // Bill calculation fields
        private decimal _subtotal;
        public decimal Subtotal
        {
            get => _subtotal;
            set
            {
                if (SetProperty(ref _subtotal, value))
                {
                    RecalculateBill();
                }
            }
        }

        private decimal _discountPercent;
        public decimal DiscountPercent
        {
            get => _discountPercent;
            set
            {
                if (SetProperty(ref _discountPercent, value))
                {
                    RecalculateBill();
                }
            }
        }

        private decimal _discountAmount;
        public decimal DiscountAmount
        {
            get => _discountAmount;
            set => SetProperty(ref _discountAmount, value);
        }

        private decimal _totalAmount;
        public decimal TotalAmount
        {
            get => _totalAmount;
            set
            {
                if (SetProperty(ref _totalAmount, value))
                {
                    OnPropertyChanged(nameof(RemainingBalance));
                    UpdatePaymentsAndChange();
                    UpdateReceiptPreview();
                }
            }
        }

        // Warning flags for pending/cooking items
        private bool _hasWarningPendingCooking;
        public bool HasWarningPendingCooking
        {
            get => _hasWarningPendingCooking;
            set => SetProperty(ref _hasWarningPendingCooking, value);
        }

        private string _warningMessage = string.Empty;
        public string WarningMessage
        {
            get => _warningMessage;
            set => SetProperty(ref _warningMessage, value);
        }

        // Split Payment fields
        private decimal _cashAmount;
        public decimal CashAmount
        {
            get => _cashAmount;
            set
            {
                if (SetProperty(ref _cashAmount, value))
                {
                    UpdatePaymentsAndChange();
                }
            }
        }

        private decimal _cardAmount;
        public decimal CardAmount
        {
            get => _cardAmount;
            set
            {
                if (SetProperty(ref _cardAmount, value))
                {
                    UpdatePaymentsAndChange();
                }
            }
        }

        private decimal _bankTransferAmount;
        public decimal BankTransferAmount
        {
            get => _bankTransferAmount;
            set
            {
                if (SetProperty(ref _bankTransferAmount, value))
                {
                    UpdatePaymentsAndChange();
                }
            }
        }

        public decimal TotalPaid => CashAmount + CardAmount + BankTransferAmount;

        public decimal RemainingBalance => Math.Max(0, TotalAmount - TotalPaid);

        private decimal _changeAmount;
        public decimal ChangeAmount
        {
            get => _changeAmount;
            set => SetProperty(ref _changeAmount, value);
        }

        // Next table status after checkout
        private List<string> _tableStatuses = new() { "needs_cleaning", "available" };
        public List<string> TableStatuses => _tableStatuses;

        private string _selectedNextTableStatus = "needs_cleaning";
        public string SelectedNextTableStatus
        {
            get => _selectedNextTableStatus;
            set => SetProperty(ref _selectedNextTableStatus, value);
        }

        // Visual Receipt Preview Text
        private string _receiptPreview = string.Empty;
        public string ReceiptPreview
        {
            get => _receiptPreview;
            set => SetProperty(ref _receiptPreview, value);
        }

        private string _statusMessage = string.Empty;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private string _statusColor = "#2ECC71"; // green or red
        public string StatusColor
        {
            get => _statusColor;
            set => SetProperty(ref _statusColor, value);
        }

        // Commands
        public ICommand LoadTablesCommand { get; }
        public ICommand SearchCustomerCommand { get; }
        public ICommand ClearCustomerCommand { get; }
        public ICommand PayFullCashCommand { get; }
        public ICommand PayFullCardCommand { get; }
        public ICommand PayFullTransferCommand { get; }
        public ICommand CheckoutCommand { get; }
        public ICommand ClearPaymentCommand { get; }

        public CheckoutViewModel()
        {
            _tableService = new TableService();
            _orderService = new OrderService();
            _customerService = new CustomerService();
            _invoiceService = new InvoiceService();

            LoadTablesCommand = new RelayCommand(LoadTables);
            SearchCustomerCommand = new RelayCommand(SearchCustomer);
            ClearCustomerCommand = new RelayCommand(ClearCustomer);
            PayFullCashCommand = new RelayCommand(ExecutePayFullCash);
            PayFullCardCommand = new RelayCommand(ExecutePayFullCard);
            PayFullTransferCommand = new RelayCommand(ExecutePayFullTransfer);
            CheckoutCommand = new RelayCommand(ExecuteCheckout, CanExecuteCheckout);
            ClearPaymentCommand = new RelayCommand(ExecuteClearPayment);

            LoadTables();
        }

        private void LoadTables()
        {
            var rawTables = _tableService.GetAllTables() ?? new List<RestaurantTable>();
            Tables = new ObservableCollection<RestaurantTable>(rawTables);
            
            // Re-select previously selected table if still active, otherwise clear
            if (SelectedTable != null)
            {
                var match = Tables.FirstOrDefault(t => t.TableId == SelectedTable.TableId);
                SelectedTable = match;
            }
        }

        private void OnTableSelected()
        {
            if (SelectedTable == null)
            {
                ActiveSession = null;
                SessionItems.Clear();
                SelectedCustomer = null;
                Subtotal = 0;
                ExecuteClearPayment();
                WarningMessage = string.Empty;
                HasWarningPendingCooking = false;
                ReceiptPreview = string.Empty;
                return;
            }

            // Get active session
            var session = _tableService.GetActiveSessionByTableId(SelectedTable.TableId);
            ActiveSession = session;
            ExecuteClearPayment();

            if (session != null)
            {
                // Load customer
                if (session.CustomerId.HasValue)
                {
                    var allCusts = _customerService.GetAllCustomers();
                    SelectedCustomer = allCusts.FirstOrDefault(c => c.CustomerId == session.CustomerId.Value);
                }
                else
                {
                    SelectedCustomer = null;
                }

                // Load items
                LoadSessionItems(session.SessionId);
            }
            else
            {
                SessionItems.Clear();
                SelectedCustomer = null;
                Subtotal = 0;
                WarningMessage = string.Empty;
                HasWarningPendingCooking = false;
                ReceiptPreview = string.Empty;
            }
        }

        private void LoadSessionItems(int sessionId)
        {
            var rawItems = _orderService.GetOrderItemsBySessionId(sessionId) ?? new List<OrderItem>();
            var activeDishes = _orderService.GetActiveDishes() ?? new List<Dish>();

            var list = new List<OrderedItemViewModel>();
            decimal calcSubtotal = 0;
            int pendingCount = 0;
            int cookingCount = 0;

            foreach (var item in rawItems)
            {
                // We show all items, including cancelled, pending, cooking, ready, served
                var dish = activeDishes.FirstOrDefault(d => d.DishId == item.DishId);
                string dishName = dish?.DishName ?? $"Món ăn #{item.DishId}";
                decimal price = dish?.Price ?? item.UnitPrice;

                list.Add(new OrderedItemViewModel(item, dishName, price));

                if (item.Status.Equals("ready", StringComparison.OrdinalIgnoreCase) || 
                    item.Status.Equals("served", StringComparison.OrdinalIgnoreCase))
                {
                    calcSubtotal += item.Quantity * price;
                }
                else if (item.Status.Equals("pending", StringComparison.OrdinalIgnoreCase))
                {
                    pendingCount++;
                }
                else if (item.Status.Equals("cooking", StringComparison.OrdinalIgnoreCase))
                {
                    cookingCount++;
                }
            }

            SessionItems = new ObservableCollection<OrderedItemViewModel>(list);
            Subtotal = calcSubtotal;

            if (pendingCount > 0 || cookingCount > 0)
            {
                HasWarningPendingCooking = true;
                WarningMessage = $"⚠️ Cảnh báo: Bàn có {pendingCount} món chờ nấu và {cookingCount} món đang nấu dở!";
            }
            else
            {
                HasWarningPendingCooking = false;
                WarningMessage = string.Empty;
            }

            UpdateReceiptPreview();
        }

        private void SearchCustomer()
        {
            if (string.IsNullOrWhiteSpace(CustomerPhoneSearch))
            {
                CustomerSearchMessage = "Vui lòng nhập số điện thoại!";
                return;
            }

            var customer = _customerService.GetCustomerByPhone(CustomerPhoneSearch);
            if (customer != null)
            {
                SelectedCustomer = customer;
                CustomerSearchMessage = "Tìm thấy khách hàng thành công!";
            }
            else
            {
                CustomerSearchMessage = "Không tìm thấy khách hàng!";
            }
        }

        private void ClearCustomer()
        {
            SelectedCustomer = null;
            CustomerPhoneSearch = string.Empty;
            CustomerSearchMessage = string.Empty;
        }

        private void RecalculateBill()
        {
            // Apply discount based on membership tier
            decimal tierDiscountPercent = 0;
            if (SelectedCustomer != null)
            {
                string tier = SelectedCustomer.MembershipTier.ToLower();
                if (tier == "vip")
                {
                    tierDiscountPercent = 10;
                }
                else if (tier == "vip_gold")
                {
                    tierDiscountPercent = 20;
                }
            }

            DiscountPercent = tierDiscountPercent;
            DiscountAmount = Subtotal * (DiscountPercent / 100);
            TotalAmount = Math.Max(0, Subtotal - DiscountAmount);
        }

        private void UpdatePaymentsAndChange()
        {
            OnPropertyChanged(nameof(TotalPaid));
            OnPropertyChanged(nameof(RemainingBalance));

            if (TotalPaid >= TotalAmount)
            {
                ChangeAmount = TotalPaid - TotalAmount;
            }
            else
            {
                ChangeAmount = 0;
            }
        }

        private void ExecutePayFullCash()
        {
            CashAmount = TotalAmount;
            CardAmount = 0;
            BankTransferAmount = 0;
        }

        private void ExecutePayFullCard()
        {
            CashAmount = 0;
            CardAmount = TotalAmount;
            BankTransferAmount = 0;
        }

        private void ExecutePayFullTransfer()
        {
            CashAmount = 0;
            CardAmount = 0;
            BankTransferAmount = TotalAmount;
        }

        private void ExecuteClearPayment()
        {
            CashAmount = 0;
            CardAmount = 0;
            BankTransferAmount = 0;
        }

        private bool CanExecuteCheckout()
        {
            return SelectedTable != null && 
                   ActiveSession != null && 
                   TotalPaid >= TotalAmount && 
                   TotalAmount > 0;
        }

        private void ExecuteCheckout()
        {
            if (!CanExecuteCheckout() || ActiveSession == null || SelectedTable == null)
            {
                ShowStatus("Thông tin thanh toán không hợp lệ!", false);
                return;
            }

            // Create Invoice
            var invoice = new Invoice
            {
                SessionId = ActiveSession.SessionId,
                Subtotal = Subtotal,
                Discount = DiscountAmount,
                TotalAmount = TotalAmount,
                CashierEmployeeId = AuthService.Instance.CurrentUser?.EmployeeId
            };

            // Create Payment Details
            var payments = new List<PaymentDetail>();
            if (CashAmount > 0)
            {
                // For cash, if customer paid more, we record actual payment up to the total amount, or log the actual split.
                // Normally, we record the amount that covers the bill (or the portion assigned to Cash).
                // Let's cap Cash payment to TotalAmount - Card - Transfer so it records exact bill paid
                decimal assignedCash = Math.Min(CashAmount, TotalAmount - CardAmount - BankTransferAmount);
                if (assignedCash > 0)
                {
                    payments.Add(new PaymentDetail { Method = "cash", Amount = assignedCash });
                }
            }
            if (CardAmount > 0)
            {
                payments.Add(new PaymentDetail { Method = "card", Amount = CardAmount });
            }
            if (BankTransferAmount > 0)
            {
                payments.Add(new PaymentDetail { Method = "bank_transfer", Amount = BankTransferAmount });
            }

            // Find all tables merged in this session (usually just SelectedTable, but we query table_sessions)
            List<int> tableIds = new List<int> { SelectedTable.TableId };
            using (var context = new Data.RestaurantPOSDbContext())
            {
                var mergedTables = context.TableSessions
                    .Where(ts => ts.SessionId == ActiveSession.SessionId)
                    .Select(ts => ts.TableId)
                    .ToList();
                if (mergedTables.Any())
                {
                    tableIds = mergedTables;
                }
            }

            // Loyalty points calculation: 1 point for each 10,000 VND spent
            int pointsEarned = (int)(TotalAmount / 10000);

            // Execute in Transaction
            bool success = _invoiceService.CreateInvoiceAndCloseSession(
                invoice, 
                payments, 
                tableIds, 
                SelectedNextTableStatus, 
                SelectedCustomer, 
                pointsEarned);

            if (success)
            {
                ShowStatus($"Thanh toán thành công {TotalAmount:N0}đ cho {SelectedTable.TableName}!", true);
                
                // Clear state
                SelectedTable = null;
                LoadTables();
            }
            else
            {
                ShowStatus("Gặp lỗi hệ thống khi lưu hóa đơn!", false);
            }
        }

        private void ShowStatus(string message, bool isSuccess)
        {
            StatusMessage = message;
            StatusColor = isSuccess ? "#2ECC71" : "#E74C3C";
        }

        private void UpdateReceiptPreview()
        {
            if (ActiveSession == null || SelectedTable == null)
            {
                ReceiptPreview = string.Empty;
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("      🍽️ HÓA ĐƠN THANH TOÁN 🍽️");
            sb.AppendLine("          RESTAURANT POS");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine($"Bàn: {SelectedTable.TableName}");
            sb.AppendLine($"Giờ vào: {ActiveSession.OpenedAt:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"Giờ ra:  {DateTime.Now:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"Nhân viên: {AuthService.Instance.CurrentUser?.FullName ?? "Thu ngân"}");
            sb.AppendLine($"Khách hàng: {CustomerDisplayName}");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine("Món ăn                  SL   Đ.Giá  T.Tiền");
            sb.AppendLine("----------------------------------------");

            foreach (var item in SessionItems)
            {
                // Only count ready or served in receipt summary representation
                if (item.Status.Equals("ready", StringComparison.OrdinalIgnoreCase) || 
                    item.Status.Equals("served", StringComparison.OrdinalIgnoreCase))
                {
                    string dishNameLine = item.DishName;
                    if (dishNameLine.Length > 22)
                        dishNameLine = dishNameLine.Substring(0, 19) + "...";
                    
                    sb.AppendLine($"{dishNameLine,-22} {item.Quantity,2} {item.Price,7:N0} {item.Subtotal,7:N0}");
                }
            }

            sb.AppendLine("----------------------------------------");
            sb.AppendLine($"Cộng tiền món:              {Subtotal,12:N0}đ");
            if (DiscountAmount > 0)
            {
                sb.AppendLine($"Chiết khấu ({DiscountPercent}%):          -{DiscountAmount,12:N0}đ");
            }
            sb.AppendLine("----------------------------------------");
            sb.AppendLine($"TỔNG TIỀN PHẢI THANH TOÁN:  {TotalAmount,12:N0}đ");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine("Hình thức thanh toán:");
            if (CashAmount > 0)
                sb.AppendLine($"  - Tiền mặt:               {CashAmount,12:N0}đ");
            if (CardAmount > 0)
                sb.AppendLine($"  - Quẹt thẻ:               {CardAmount,12:N0}đ");
            if (BankTransferAmount > 0)
                sb.AppendLine($"  - Chuyển khoản:           {BankTransferAmount,12:N0}đ");
            
            if (TotalPaid >= TotalAmount)
            {
                sb.AppendLine($"Tiền thối lại:              {ChangeAmount,12:N0}đ");
            }

            if (SelectedCustomer != null)
            {
                int pointsEarned = (int)(TotalAmount / 10000);
                sb.AppendLine("----------------------------------------");
                sb.AppendLine($"Điểm tích lũy hiện tại:      {SelectedCustomer.LoyaltyPoints,10}");
                sb.AppendLine($"Điểm tích lũy nhận thêm:     +{pointsEarned,9}");
            }

            sb.AppendLine("----------------------------------------");
            sb.AppendLine("        CẢM ƠN QUÝ KHÁCH & HẸN GẶP LẠI");
            sb.AppendLine("               POWERED BY GROUP 4");

            ReceiptPreview = sb.ToString();
        }
    }
}
