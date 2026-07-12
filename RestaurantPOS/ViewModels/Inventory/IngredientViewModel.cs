using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using RestaurantPOS.Models;
using RestaurantPOS.MVVM;
using RestaurantPOS.Services;

namespace RestaurantPOS.ViewModels.Inventory
{
    public class IngredientViewModel : ViewModelBase
    {
        private readonly IIngredientService _ingredientService;

        private ObservableCollection<Ingredient> _ingredients;
        public ObservableCollection<Ingredient> Ingredients
        {
            get => _ingredients;
            set => SetProperty(ref _ingredients, value);
        }

        private Ingredient _selectedIngredient;
        public Ingredient SelectedIngredient
        {
            get => _selectedIngredient;
            set
            {
                if (SetProperty(ref _selectedIngredient, value))
                {
                    LoadSelectedIngredientDetails();
                }
            }
        }

        private string _ingredientName;
        public string IngredientName
        {
            get => _ingredientName;
            set => SetProperty(ref _ingredientName, value);
        }

        private string _unit;
        public string Unit
        {
            get => _unit;
            set => SetProperty(ref _unit, value);
        }

        private decimal _stockQuantity;
        public decimal StockQuantity
        {
            get => _stockQuantity;
            set => SetProperty(ref _stockQuantity, value);
        }

        private decimal? _minStockAlert;
        public decimal? MinStockAlert
        {
            get => _minStockAlert;
            set => SetProperty(ref _minStockAlert, value);
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

        public bool IsEditMode => SelectedIngredient != null;

        public ICommand LoadIngredientsCommand { get; }
        public ICommand SaveIngredientCommand { get; }
        public ICommand DeleteIngredientCommand { get; }
        public ICommand ClearFormCommand { get; }

        public IngredientViewModel()
        {
            _ingredientService = new IngredientService();
            Ingredients = new ObservableCollection<Ingredient>();

            LoadIngredientsCommand = new RelayCommand(LoadIngredients);
            SaveIngredientCommand = new RelayCommand(ExecuteSave);
            DeleteIngredientCommand = new RelayCommand(ExecuteDelete, () => IsEditMode);
            ClearFormCommand = new RelayCommand(ClearForm);

            LoadIngredients();
        }

        private void LoadIngredients()
        {
            try
            {
                var list = _ingredientService.GetAllIngredients();
                Ingredients = new ObservableCollection<Ingredient>(list);
                ShowStatus("Đã tải danh sách nguyên liệu.", true);
            }
            catch (Exception ex)
            {
                ShowStatus($"Lỗi tải dữ liệu: {ex.Message}", false);
            }
        }

        private void LoadSelectedIngredientDetails()
        {
            if (SelectedIngredient != null)
            {
                IngredientName = SelectedIngredient.IngredientName;
                Unit = SelectedIngredient.Unit;
                StockQuantity = SelectedIngredient.StockQuantity;
                MinStockAlert = SelectedIngredient.MinStockAlert;
            }
            else
            {
                ClearInputs();
            }
            OnPropertyChanged(nameof(IsEditMode));
            CommandManager.InvalidateRequerySuggested();
        }

        private void ExecuteSave()
        {
            if (string.IsNullOrWhiteSpace(IngredientName))
            {
                ShowStatus("Vui lòng nhập tên nguyên liệu!", false);
                return;
            }
            if (string.IsNullOrWhiteSpace(Unit))
            {
                ShowStatus("Vui lòng nhập đơn vị tính!", false);
                return;
            }
            if (StockQuantity < 0)
            {
                ShowStatus("Số lượng tồn kho không thể âm!", false);
                return;
            }
            if (MinStockAlert.HasValue && MinStockAlert.Value < 0)
            {
                ShowStatus("Mức cảnh báo tồn kho không thể âm!", false);
                return;
            }

            try
            {
                if (IsEditMode)
                {
                    SelectedIngredient.IngredientName = IngredientName;
                    SelectedIngredient.Unit = Unit;
                    SelectedIngredient.StockQuantity = StockQuantity;
                    SelectedIngredient.MinStockAlert = MinStockAlert;

                    if (_ingredientService.UpdateIngredient(SelectedIngredient))
                    {
                        ShowStatus("Cập nhật nguyên liệu thành công!", true);
                        LoadIngredients();
                        ClearForm();
                    }
                    else
                    {
                        ShowStatus("Cập nhật nguyên liệu thất bại!", false);
                    }
                }
                else
                {
                    var newIngredient = new Ingredient
                    {
                        IngredientName = IngredientName,
                        Unit = Unit,
                        StockQuantity = StockQuantity,
                        MinStockAlert = MinStockAlert
                    };

                    if (_ingredientService.AddIngredient(newIngredient))
                    {
                        ShowStatus("Thêm nguyên liệu thành công!", true);
                        LoadIngredients();
                        ClearForm();
                    }
                    else
                    {
                        ShowStatus("Thêm nguyên liệu thất bại!", false);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"Lỗi hệ thống: {ex.Message}", false);
            }
        }

        private void ExecuteDelete()
        {
            if (SelectedIngredient == null) return;

            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa nguyên liệu '{SelectedIngredient.IngredientName}' không?", 
                "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (_ingredientService.DeleteIngredient(SelectedIngredient.IngredientId))
                    {
                        ShowStatus("Xóa nguyên liệu thành công!", true);
                        LoadIngredients();
                        ClearForm();
                    }
                    else
                    {
                        ShowStatus("Xóa nguyên liệu thất bại! Nguyên liệu này có thể đã được gán công thức món ăn.", false);
                    }
                }
                catch (Exception ex)
                {
                    ShowStatus($"Lỗi hệ thống: {ex.Message}", false);
                }
            }
        }

        private void ClearForm()
        {
            SelectedIngredient = null;
            ClearInputs();
        }

        private void ClearInputs()
        {
            IngredientName = string.Empty;
            Unit = string.Empty;
            StockQuantity = 0;
            MinStockAlert = null;
        }

        private void ShowStatus(string message, bool isSuccess)
        {
            StatusMessage = message;
            StatusColor = isSuccess ? "#2ECC71" : "#E74C3C";
        }
    }
}
