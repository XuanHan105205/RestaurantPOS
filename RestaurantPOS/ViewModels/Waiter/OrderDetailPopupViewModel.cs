using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using RestaurantPOS.Models;
using RestaurantPOS.MVVM;
using RestaurantPOS.Services;

namespace RestaurantPOS.ViewModels.Waiter
{
    public class OrderedItemViewModel : ViewModelBase
    {
        public OrderItem OrderItem { get; }
        public string DishName { get; }
        public decimal Price { get; }

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

        public decimal Subtotal => Price * Quantity;
        public string Status => OrderItem.Status;

        public bool IsPending => Status.Equals("pending", StringComparison.OrdinalIgnoreCase);
        public bool IsNotPending => !IsPending;

        public string StatusText
        {
            get
            {
                return Status.ToLower() switch
                {
                    "pending" => "⏳ Chờ chế biến",
                    "cooking" => "🍳 Đang nấu",
                    "ready" => "✅ Đã nấu xong",
                    "served" => "🍽️ Đã phục vụ",
                    "cancelled" => "❌ Đã hủy",
                    _ => Status
                };
            }
        }

        public OrderedItemViewModel(OrderItem orderItem, string dishName, decimal price)
        {
            OrderItem = orderItem;
            DishName = dishName;
            Price = price;
            _quantity = orderItem.Quantity;
            _note = orderItem.Note ?? "";
        }

        public void RefreshStatus()
        {
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(IsPending));
            OnPropertyChanged(nameof(IsNotPending));
        }
    }

    public class OrderDetailPopupViewModel : ViewModelBase
    {
        private readonly IOrderService _orderService;
        public DiningSession ActiveSession { get; }
        public string TableName { get; }

        private ObservableCollection<OrderedItemViewModel> _orderedItems;
        public ObservableCollection<OrderedItemViewModel> OrderedItems
        {
            get => _orderedItems;
            set => SetProperty(ref _orderedItems, value);
        }

        private OrderedItemViewModel? _selectedItem;
        public OrderedItemViewModel? SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public decimal SessionTotal => OrderedItems?.Where(item => item.Status != "cancelled").Sum(item => item.Subtotal) ?? 0;

        public ICommand LoadItemsCommand { get; }
        public ICommand IncrementCommand { get; }
        public ICommand DecrementCommand { get; }
        public ICommand SaveNoteCommand { get; }
        public ICommand CancelItemCommand { get; }

        public OrderDetailPopupViewModel(DiningSession session, string tableName)
        {
            ActiveSession = session;
            TableName = tableName;
            _orderService = new OrderService();
            _orderedItems = new ObservableCollection<OrderedItemViewModel>();

            LoadItemsCommand = new RelayCommand(LoadItems);
            IncrementCommand = new RelayCommand<OrderedItemViewModel>(IncrementQuantity);
            DecrementCommand = new RelayCommand<OrderedItemViewModel>(DecrementQuantity);
            SaveNoteCommand = new RelayCommand<OrderedItemViewModel>(SaveNote);
            CancelItemCommand = new RelayCommand<OrderedItemViewModel>(CancelItem);

            LoadItems();
        }

        private void LoadItems()
        {
            var rawItems = _orderService.GetOrderItemsBySessionId(ActiveSession.SessionId);
            var dishes = _orderService.GetActiveDishes();

            var list = new List<OrderedItemViewModel>();
            foreach (var item in rawItems)
            {
                var dish = dishes.FirstOrDefault(d => d.DishId == item.DishId);
                string dishName = dish?.DishName ?? $"Món ăn #{item.DishId}";
                decimal price = dish?.Price ?? item.UnitPrice;

                list.Add(new OrderedItemViewModel(item, dishName, price));
            }

            OrderedItems = new ObservableCollection<OrderedItemViewModel>(list);
            OnPropertyChanged(nameof(SessionTotal));
        }

        private void IncrementQuantity(OrderedItemViewModel item)
        {
            if (item == null || !item.IsPending) return;

            item.Quantity++;
            item.OrderItem.Quantity = item.Quantity;
            item.OrderItem.StatusUpdatedAt = DateTime.Now;

            _orderService.UpdateOrderItem(item.OrderItem);
            OnPropertyChanged(nameof(SessionTotal));
        }

        private void DecrementQuantity(OrderedItemViewModel item)
        {
            if (item == null || !item.IsPending) return;

            if (item.Quantity > 1)
            {
                item.Quantity--;
                item.OrderItem.Quantity = item.Quantity;
                item.OrderItem.StatusUpdatedAt = DateTime.Now;
                _orderService.UpdateOrderItem(item.OrderItem);
            }
            else
            {
                CancelItem(item);
            }
            OnPropertyChanged(nameof(SessionTotal));
        }

        private void SaveNote(OrderedItemViewModel item)
        {
            if (item == null || !item.IsPending) return;

            item.OrderItem.Note = item.Note;
            item.OrderItem.StatusUpdatedAt = DateTime.Now;

            if (_orderService.UpdateOrderItem(item.OrderItem))
            {
                MessageBox.Show("Cập nhật ghi chú thành công!");
            }
            else
            {
                MessageBox.Show("Không thể cập nhật ghi chú.");
            }
        }

        private void CancelItem(OrderedItemViewModel item)
        {
            if (item == null || !item.IsPending) return;

            var result = MessageBox.Show($"Bạn có chắc chắn muốn hủy món '{item.DishName}'?", "Xác nhận hủy món", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                item.OrderItem.Status = "cancelled";
                item.OrderItem.StatusUpdatedAt = DateTime.Now;
                _orderService.UpdateOrderItem(item.OrderItem);
                item.RefreshStatus();
                LoadItems();
            }
        }
    }
}
