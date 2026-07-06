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

        private ObservableCollection<KitchenOrderItemDto> _pendingItems;
        public ObservableCollection<KitchenOrderItemDto> PendingItems
        {
            get => _pendingItems;
            set => SetProperty(ref _pendingItems, value);
        }

        private ObservableCollection<KitchenOrderItemDto> _cookingItems;
        public ObservableCollection<KitchenOrderItemDto> CookingItems
        {
            get => _cookingItems;
            set => SetProperty(ref _cookingItems, value);
        }

        private ObservableCollection<KitchenOrderItemDto> _readyItems;
        public ObservableCollection<KitchenOrderItemDto> ReadyItems
        {
            get => _readyItems;
            set => SetProperty(ref _readyItems, value);
        }

        private ObservableCollection<KitchenOrderItemDto> _servedItems;
        public ObservableCollection<KitchenOrderItemDto> ServedItems
        {
            get => _servedItems;
            set => SetProperty(ref _servedItems, value);
        }

        private string _lastUpdatedText;
        public string LastUpdatedText
        {
            get => _lastUpdatedText;
            set => SetProperty(ref _lastUpdatedText, value);
        }

        public ICommand StartCookingCommand { get; }
        public ICommand MarkReadyCommand { get; }
        public ICommand MarkServedCommand { get; }
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
            if (item != null)
            {
                if (_kitchenService.UpdateOrderItemStatus(item.OrderItemId, "cooking"))
                {
                    LoadData();
                }
            }
        }

        private void ExecuteMarkReady(KitchenOrderItemDto item)
        {
            if (item != null)
            {
                if (_kitchenService.UpdateOrderItemStatus(item.OrderItemId, "ready"))
                {
                    LoadData();
                }
            }
        }

        private void ExecuteMarkServed(KitchenOrderItemDto item)
        {
            if (item != null)
            {
                if (_kitchenService.UpdateOrderItemStatus(item.OrderItemId, "served"))
                {
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
