using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using RestaurantPOS.Models;
using RestaurantPOS.MVVM;
using RestaurantPOS.Services;

namespace RestaurantPOS.ViewModels.Inventory
{
    public class StockReceiptViewModel : ViewModelBase
    {
        private readonly IStockService _stockService;
        private readonly IIngredientService _ingredientService;

        private ObservableCollection<StockReceiptDto> _receipts;
        public ObservableCollection<StockReceiptDto> Receipts
        {
            get => _receipts;
            set => SetProperty(ref _receipts, value);
        }

        private List<Ingredient> _ingredients;
        public List<Ingredient> Ingredients
        {
            get => _ingredients;
            set => SetProperty(ref _ingredients, value);
        }

        private Ingredient _selectedIngredient;
        public Ingredient SelectedIngredient
        {
            get => _selectedIngredient;
            set => SetProperty(ref _selectedIngredient, value);
        }

        private decimal _quantity;
        public decimal Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }

        private decimal? _unitCost;
        public decimal? UnitCost
        {
            get => _unitCost;
            set => SetProperty(ref _unitCost, value);
        }

        private string _supplier;
        public string Supplier
        {
            get => _supplier;
            set => SetProperty(ref _supplier, value);
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        private string _statusColor;
        public string StatusColor
        {
            get => _statusColor;
            set => SetProperty(ref _statusColor, value);
        }

        public ICommand LoadReceiptsCommand { get; }
        public ICommand SaveReceiptCommand { get; }
        public ICommand ClearFormCommand { get; }

        public StockReceiptViewModel()
        {
            _stockService = new StockService();
            _ingredientService = new IngredientService();
            Receipts = new ObservableCollection<StockReceiptDto>();

            LoadReceiptsCommand = new RelayCommand(LoadData);
            SaveReceiptCommand = new RelayCommand(ExecuteSave);
            ClearFormCommand = new RelayCommand(ClearForm);

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // Load Ingredients for ComboBox
                Ingredients = _ingredientService.GetAllIngredients();

                // Load Receipts and map to DTOs
                var rawReceipts = _stockService.GetAllReceipts();
                var dbIngredients = _ingredientService.GetAllIngredients().ToDictionary(i => i.IngredientId);

                using (var context = new Data.RestaurantPOSDbContext())
                {
                    var dbEmployees = context.Employees.ToDictionary(e => e.EmployeeId);

                    var mapped = rawReceipts.Select(r => new StockReceiptDto
                    {
                        ReceiptId = r.ReceiptId,
                        IngredientName = dbIngredients.TryGetValue(r.IngredientId, out var ing) ? ing.IngredientName : "Không xác định",
                        Unit = ing?.Unit ?? "",
                        Quantity = r.Quantity,
                        UnitCost = r.UnitCost,
                        ReceivedAt = r.ReceivedAt,
                        ReceivedByEmployeeName = r.ReceivedByEmployeeId.HasValue && dbEmployees.TryGetValue(r.ReceivedByEmployeeId.Value, out var emp) ? emp.FullName : "Hệ thống",
                        Supplier = r.Supplier ?? "-"
                    }).ToList();

                    Receipts = new ObservableCollection<StockReceiptDto>(mapped);
                }

                ShowStatus("Đã tải danh sách phiếu nhập kho.", true);
            }
            catch (Exception ex)
            {
                ShowStatus($"Lỗi tải dữ liệu: {ex.Message}", false);
            }
        }

        private void ExecuteSave()
        {
            if (SelectedIngredient == null)
            {
                ShowStatus("Vui lòng chọn nguyên liệu cần nhập!", false);
                return;
            }
            if (Quantity <= 0)
            {
                ShowStatus("Số lượng nhập kho phải lớn hơn 0!", false);
                return;
            }
            if (UnitCost.HasValue && UnitCost.Value < 0)
            {
                ShowStatus("Đơn giá nhập kho không thể âm!", false);
                return;
            }

            try
            {
                int? currentEmployeeId = AuthService.Instance.CurrentUser?.EmployeeId;

                var newReceipt = new StockReceipt
                {
                    IngredientId = SelectedIngredient.IngredientId,
                    Quantity = Quantity,
                    UnitCost = UnitCost,
                    Supplier = Supplier,
                    ReceivedByEmployeeId = currentEmployeeId
                };

                if (_stockService.AddStockReceipt(newReceipt))
                {
                    ShowStatus($"Nhập kho nguyên liệu '{SelectedIngredient.IngredientName}' thành công!", true);
                    LoadData();
                    ClearForm();
                }
                else
                {
                    ShowStatus("Nhập kho nguyên liệu thất bại!", false);
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"Lỗi hệ thống: {ex.Message}", false);
            }
        }

        private void ClearForm()
        {
            SelectedIngredient = null;
            Quantity = 0;
            UnitCost = null;
            Supplier = string.Empty;
        }

        private void ShowStatus(string message, bool isSuccess)
        {
            StatusMessage = message;
            StatusColor = isSuccess ? "#2ECC71" : "#E74C3C";
        }
    }

    public class StockReceiptDto
    {
        public int ReceiptId { get; set; }
        public string IngredientName { get; set; }
        public string Unit { get; set; }
        public decimal Quantity { get; set; }
        public decimal? UnitCost { get; set; }
        public decimal? TotalCost => Quantity * UnitCost;
        public DateTime ReceivedAt { get; set; }
        public string ReceivedByEmployeeName { get; set; }
        public string Supplier { get; set; }
    }
}
