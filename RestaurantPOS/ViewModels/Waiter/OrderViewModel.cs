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
    public class CartItem : ViewModelBase
    {
        public Dish Dish { get; }
        
        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (SetProperty(ref _quantity, value))
                {
                    OnPropertyChanged(nameof(Subtotal));
                }
            }
        }

        private string _note = "";
        public string Note
        {
            get => _note;
            set => SetProperty(ref _note, value);
        }

        public decimal Subtotal => Dish.Price * Quantity;

        public CartItem(Dish dish, int quantity = 1, string note = "")
        {
            Dish = dish;
            _quantity = quantity;
            _note = note;
        }
    }

    public class OrderViewModel : ViewModelBase
    {
        private readonly IOrderService _orderService;

        public RestaurantTable SelectedTable { get; }
        public DiningSession ActiveSession { get; }

        private ObservableCollection<Category> _categories = new();
        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        private Category _selectedCategory = new();
        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    ApplyFilter();
                }
            }
        }

        private List<Dish> _allDishes = new();

        private ObservableCollection<Dish> _filteredDishes = new();
        public ObservableCollection<Dish> FilteredDishes
        {
            get => _filteredDishes;
            set => SetProperty(ref _filteredDishes, value);
        }

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    ApplyFilter();
                }
            }
        }

        private ObservableCollection<CartItem> _cart = new();
        public ObservableCollection<CartItem> Cart
        {
            get => _cart;
            set => SetProperty(ref _cart, value);
        }

        public decimal CartTotal => Cart.Sum(item => item.Subtotal);

        public ICommand SelectCategoryCommand { get; }
        public ICommand AddToCartCommand { get; }
        public ICommand RemoveFromCartCommand { get; }
        public ICommand IncrementCartQuantityCommand { get; }
        public ICommand DecrementCartQuantityCommand { get; }
        public ICommand ConfirmOrderCommand { get; }
        public ICommand CancelOrderCommand { get; }

        public OrderViewModel(RestaurantTable table, DiningSession session)
        {
            SelectedTable = table;
            ActiveSession = session;
            _orderService = new OrderService();
            _cart = new ObservableCollection<CartItem>();
            _cart.CollectionChanged += (s, e) => OnPropertyChanged(nameof(CartTotal));

            SelectCategoryCommand = new RelayCommand<Category>(category => SelectedCategory = category);
            AddToCartCommand = new RelayCommand<Dish>(AddToCart);
            RemoveFromCartCommand = new RelayCommand<CartItem>(RemoveFromCart);
            IncrementCartQuantityCommand = new RelayCommand<CartItem>(IncrementCartQuantity);
            DecrementCartQuantityCommand = new RelayCommand<CartItem>(DecrementCartQuantity);
            ConfirmOrderCommand = new RelayCommand(ConfirmOrder);
            CancelOrderCommand = new RelayCommand(CancelOrder);

            LoadMenu();
        }

        private void LoadMenu()
        {
            var cats = _orderService.GetAllCategories();
            
            // Insert a virtual "Tất cả" (All) category at the top
            var allCategory = new Category { CategoryId = -1, CategoryName = "Tất cả" };
            cats.Insert(0, allCategory);

            Categories = new ObservableCollection<Category>(cats);
            _selectedCategory = allCategory;

            _allDishes = _orderService.GetActiveDishes();
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (_allDishes == null) return;

            IEnumerable<Dish> filtered = _allDishes;

            if (SelectedCategory != null && SelectedCategory.CategoryId != -1)
            {
                filtered = filtered.Where(d => d.CategoryId == SelectedCategory.CategoryId);
            }

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                string searchLower = SearchText.ToLower();
                filtered = filtered.Where(d => d.DishName.ToLower().Contains(searchLower));
            }

            FilteredDishes = new ObservableCollection<Dish>(filtered);
        }

        private void AddToCart(Dish dish)
        {
            if (dish == null) return;

            var existingItem = Cart.FirstOrDefault(i => i.Dish.DishId == dish.DishId);
            if (existingItem != null)
            {
                existingItem.Quantity++;
            }
            else
            {
                var newItem = new CartItem(dish);
                newItem.PropertyChanged += (s, e) => OnPropertyChanged(nameof(CartTotal));
                Cart.Add(newItem);
            }
            OnPropertyChanged(nameof(CartTotal));
        }

        private void RemoveFromCart(CartItem item)
        {
            if (item == null) return;
            Cart.Remove(item);
        }

        private void IncrementCartQuantity(CartItem item)
        {
            if (item == null) return;
            item.Quantity++;
        }

        private void DecrementCartQuantity(CartItem item)
        {
            if (item == null) return;
            if (item.Quantity > 1)
            {
                item.Quantity--;
            }
            else
            {
                Cart.Remove(item);
            }
        }

        private void ConfirmOrder()
        {
            if (Cart.Count == 0)
            {
                System.Windows.MessageBox.Show("Vui lòng chọn món ăn trước khi xác nhận!");
                return;
            }

            int employeeId = AuthService.Instance.CurrentUser?.EmployeeId ?? 2; // Default to Trần Văn Hưng if not authenticated

            var orderItems = Cart.Select(item => new OrderItem
            {
                DishId = item.Dish.DishId,
                Quantity = item.Quantity,
                UnitPrice = item.Dish.Price,
                Note = item.Note,
                Status = "pending",
                StatusUpdatedAt = DateTime.Now
            }).ToList();

            bool success = _orderService.PlaceOrder(ActiveSession.SessionId, employeeId, orderItems);
            if (success)
            {
                System.Windows.MessageBox.Show("Gọi món thành công!");
                NavigationService.Instance.CurrentViewModel = new TableViewModel();
            }
            else
            {
                System.Windows.MessageBox.Show("Có lỗi xảy ra khi gọi món. Vui lòng thử lại.");
            }
        }

        private void CancelOrder()
        {
            NavigationService.Instance.CurrentViewModel = new TableViewModel();
        }
    }
}
