using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;
using RestaurantPOS.MVVM;
using RestaurantPOS.Repositories;
using RestaurantPOS.Services;

namespace RestaurantPOS.ViewModels.Kitchen
{
    public class KitchenViewModel : ViewModelBase
    {
        private readonly IKitchenService _kitchenService;
        private readonly DispatcherTimer _timer;

        private ObservableCollection<KitchenOrderItemDto> _pendingItems = new();
        public ObservableCollection<KitchenOrderItemDto> PendingItems
        {
            get => _pendingItems;
            set => SetProperty(ref _pendingItems, value);
        }

        private ObservableCollection<KitchenOrderItemDto> _cookingItems = new();
        public ObservableCollection<KitchenOrderItemDto> CookingItems
        {
            get => _cookingItems;
            set => SetProperty(ref _cookingItems, value);
        }

        private ObservableCollection<KitchenOrderItemDto> _readyItems = new();
        public ObservableCollection<KitchenOrderItemDto> ReadyItems
        {
            get => _readyItems;
            set => SetProperty(ref _readyItems, value);
        }

        private ObservableCollection<KitchenOrderItemDto> _servedItems = new();
        public ObservableCollection<KitchenOrderItemDto> ServedItems
        {
            get => _servedItems;
            set => SetProperty(ref _servedItems, value);
        }

        private string _lastUpdatedText = string.Empty;
        public string LastUpdatedText
        {
            get => _lastUpdatedText;
            set => SetProperty(ref _lastUpdatedText, value);
        }

        public ICommand StartCookingCommand { get; }
        public ICommand MarkReadyCommand { get; }
        public ICommand MarkServedCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand RefreshCommand { get; }

        public KitchenViewModel()
        {
            _kitchenService = new KitchenService();

            PendingItems = new ObservableCollection<KitchenOrderItemDto>();
            CookingItems = new ObservableCollection<KitchenOrderItemDto>();
            ReadyItems = new ObservableCollection<KitchenOrderItemDto>();
            ServedItems = new ObservableCollection<KitchenOrderItemDto>();

            StartCookingCommand = new RelayCommand<KitchenOrderItemDto>(ExecuteStartCooking);
            MarkReadyCommand = new RelayCommand<KitchenOrderItemDto>(ExecuteMarkReady);
            MarkServedCommand = new RelayCommand<KitchenOrderItemDto>(ExecuteMarkServed);
            CancelCommand = new RelayCommand<KitchenOrderItemDto>(ExecuteCancel);
            RefreshCommand = new RelayCommand(LoadData);

            // Cấu hình Polling tự động mỗi 5 giây
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(5);
            _timer.Tick += (s, e) => LoadData();
            _timer.Start();

            // Load dữ liệu lần đầu
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                // 1. Load các món đang hoạt động (Pending, Cooking, Ready)
                var activeItems = _kitchenService.GetActiveKitchenItems();

                var pending = activeItems.Where(x => x.Status == "pending").ToList();
                var cooking = activeItems.Where(x => x.Status == "cooking").ToList();
                var ready = activeItems.Where(x => x.Status == "ready").ToList();

                PendingItems = new ObservableCollection<KitchenOrderItemDto>(pending);
                CookingItems = new ObservableCollection<KitchenOrderItemDto>(cooking);
                ReadyItems = new ObservableCollection<KitchenOrderItemDto>(ready);

                // 2. Load các món đã phục vụ (Giới hạn trong vòng 30 phút gần nhất)
                var servedAllToday = _kitchenService.GetServedKitchenItemsToday();
                var servedFiltered = servedAllToday
                    .Where(x => x.StatusUpdatedAt.HasValue && 
                                (DateTime.Now - x.StatusUpdatedAt.Value).TotalMinutes <= 30)
                    .ToList();

                ServedItems = new ObservableCollection<KitchenOrderItemDto>(servedFiltered);

                LastUpdatedText = $"Cập nhật lúc: {DateTime.Now:HH:mm:ss}";
            }
            catch (Exception ex)
            {
                LastUpdatedText = $"Lỗi cập nhật: {ex.Message}";
            }
        }

        private void ExecuteStartCooking(KitchenOrderItemDto item)
        {
            if (item == null) return;
            if (_kitchenService.UpdateOrderItemStatus(item.OrderItemId, "cooking"))
            {
                LoadData();
            }
        }

        private void ExecuteMarkReady(KitchenOrderItemDto item)
        {
            if (item == null) return;
            if (_kitchenService.UpdateOrderItemStatus(item.OrderItemId, "ready"))
            {
                LoadData();
            }
            else
            {
                var missingList = _kitchenService.GetMissingIngredients(item.OrderItemId);
                string detailMessage = missingList.Any()
                    ? string.Join("\n• ", missingList)
                    : "Không xác định được nguyên liệu thiếu.";

                var choice = System.Windows.MessageBox.Show(
                    $"Không thể hoàn thành món '{item.DishName}' vì thiếu nguyên liệu tồn kho:\n\n• {detailMessage}\n\nBạn có muốn HỦY MÓN này (báo hết hàng cho Nhân viên Phục vụ) không?",
                    "Cảnh Báo Hết Nguyên Liệu Kho",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Warning);

                if (choice == System.Windows.MessageBoxResult.Yes)
                {
                    if (_kitchenService.CancelOrderItem(item.OrderItemId, "Báo hết nguyên liệu kho"))
                    {
                        System.Windows.MessageBox.Show($"Đã hủy món '{item.DishName}' và cập nhật cho nhân viên phục vụ!", "Thông báo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                        LoadData();
                    }
                }
            }
        }

        private void ExecuteMarkServed(KitchenOrderItemDto item)
        {
            if (item == null) return;
            if (_kitchenService.UpdateOrderItemStatus(item.OrderItemId, "served"))
            {
                LoadData();
            }
        }

        private void ExecuteCancel(KitchenOrderItemDto item)
        {
            if (item == null) return;

            var choice = System.Windows.MessageBox.Show(
                $"Bạn có chắc chắn muốn HỦY món '{item.DishName}' (Bàn: {item.TableName}) không?",
                "Xác nhận hủy món",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (choice == System.Windows.MessageBoxResult.Yes)
            {
                if (_kitchenService.CancelOrderItem(item.OrderItemId, "Bếp chủ động báo hết / hủy món"))
                {
                    System.Windows.MessageBox.Show($"Đã hủy món '{item.DishName}' thành công!", "Thông báo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                    LoadData();
                }
            }
        }

        // Đóng Timer khi đối tượng bị hủy để giải phóng tài nguyên
        ~KitchenViewModel()
        {
            if (_timer != null)
            {
                _timer.Stop();
            }
        }
    }
}
