using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using RestaurantPOS.MVVM;
using RestaurantPOS.Repositories;
using RestaurantPOS.Services;

namespace RestaurantPOS.ViewModels.Billing
{
    public class DishReportItem : ViewModelBase
    {
        public string DishName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Revenue { get; set; }
        public double Percentage { get; set; } // 0 to 100 for bar chart
    }

    public class PaymentMethodReportItem : ViewModelBase
    {
        public string Method { get; set; } = string.Empty;
        public string MethodText => Method.ToLower() switch
        {
            "cash" => "💵 Tiền mặt",
            "card" => "💳 Quẹt thẻ",
            "bank_transfer" => "🏦 Chuyển khoản",
            _ => Method
        };
        public decimal TotalAmount { get; set; }
        public int TransactionCount { get; set; }
        public double Percentage { get; set; } // 0 to 100 for bar chart
    }

    public class ReportDashboardViewModel : ViewModelBase
    {
        private readonly IReportService _reportService;

        // Date filters
        private DateTime _startDate = DateTime.Today.AddDays(-7);
        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        private DateTime _endDate = DateTime.Today;
        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        // Summary KPI Metrics
        private decimal _totalRevenue;
        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set => SetProperty(ref _totalRevenue, value);
        }

        private int _totalInvoices;
        public int TotalInvoices
        {
            get => _totalInvoices;
            set => SetProperty(ref _totalInvoices, value);
        }

        private decimal _totalDiscount;
        public decimal TotalDiscount
        {
            get => _totalDiscount;
            set => SetProperty(ref _totalDiscount, value);
        }

        private decimal _averageOrderValue;
        public decimal AverageOrderValue
        {
            get => _averageOrderValue;
            set => SetProperty(ref _averageOrderValue, value);
        }

        // Data collections for report lists
        private ObservableCollection<DailySalesSummaryDto> _dailySales = new();
        public ObservableCollection<DailySalesSummaryDto> DailySales
        {
            get => _dailySales;
            set => SetProperty(ref _dailySales, value);
        }

        // Visual chart structures
        private ObservableCollection<DishReportItem> _topDishes = new();
        public ObservableCollection<DishReportItem> TopDishes
        {
            get => _topDishes;
            set => SetProperty(ref _topDishes, value);
        }

        private ObservableCollection<PaymentMethodReportItem> _paymentMethods = new();
        public ObservableCollection<PaymentMethodReportItem> PaymentMethods
        {
            get => _paymentMethods;
            set => SetProperty(ref _paymentMethods, value);
        }

        // Commands
        public ICommand LoadReportCommand { get; }
        public ICommand FilterTodayCommand { get; }
        public ICommand FilterLast7DaysCommand { get; }
        public ICommand FilterThisMonthCommand { get; }

        public ReportDashboardViewModel()
        {
            _reportService = new ReportService();

            LoadReportCommand = new RelayCommand(LoadReportData);
            FilterTodayCommand = new RelayCommand(ExecuteFilterToday);
            FilterLast7DaysCommand = new RelayCommand(ExecuteFilterLast7Days);
            FilterThisMonthCommand = new RelayCommand(ExecuteFilterThisMonth);

            LoadReportData();
        }

        private void ExecuteFilterToday()
        {
            StartDate = DateTime.Today;
            EndDate = DateTime.Today;
            LoadReportData();
        }

        private void ExecuteFilterLast7Days()
        {
            StartDate = DateTime.Today.AddDays(-7);
            EndDate = DateTime.Today;
            LoadReportData();
        }

        private void ExecuteFilterThisMonth()
        {
            var today = DateTime.Today;
            StartDate = new DateTime(today.Year, today.Month, 1);
            EndDate = today;
            LoadReportData();
        }

        private void LoadReportData()
        {
            // Fetch raw view data
            var rawSales = _reportService.GetDailySalesSummary(StartDate, EndDate) ?? new List<DailySalesSummaryDto>();
            var rawDishes = _reportService.GetDailyBestSellingDishes(StartDate, EndDate) ?? new List<DailyBestSellingDishDto>();
            var rawPayments = _reportService.GetDailyPaymentBreakdown(StartDate, EndDate) ?? new List<DailyPaymentBreakdownDto>();

            // 1. Load Sales Summary Table
            DailySales = new ObservableCollection<DailySalesSummaryDto>(rawSales);

            // 2. Calculate KPI Cards
            TotalRevenue = rawSales.Sum(s => s.TotalRevenue);
            TotalInvoices = rawSales.Sum(s => s.TotalInvoices);
            TotalDiscount = rawSales.Sum(s => s.TotalDiscount);
            AverageOrderValue = TotalInvoices > 0 ? TotalRevenue / TotalInvoices : 0;

            // 3. Process Top Best Selling Dishes Chart (Grouped across date range)
            var groupedDishes = rawDishes
                .GroupBy(d => new { d.DishId, d.DishName })
                .Select(g => new DishReportItem
                {
                    DishName = g.Key.DishName,
                    Quantity = g.Sum(x => x.TotalQuantity),
                    Revenue = g.Sum(x => x.TotalRevenue)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(7) // Top 7 dishes
                .ToList();

            int maxQuantity = groupedDishes.Any() ? groupedDishes.Max(x => x.Quantity) : 0;
            foreach (var item in groupedDishes)
            {
                item.Percentage = maxQuantity > 0 ? ((double)item.Quantity / maxQuantity) * 100 : 0;
            }
            TopDishes = new ObservableCollection<DishReportItem>(groupedDishes);

            // 4. Process Payment Methods Chart (Grouped across date range)
            var groupedPayments = rawPayments
                .GroupBy(p => p.PaymentMethod)
                .Select(g => new PaymentMethodReportItem
                {
                    Method = g.Key,
                    TotalAmount = g.Sum(x => x.TotalAmount),
                    TransactionCount = g.Sum(x => x.TransactionCount)
                })
                .OrderByDescending(x => x.TotalAmount)
                .ToList();

            decimal maxPaymentAmount = groupedPayments.Any() ? groupedPayments.Max(x => x.TotalAmount) : 0;
            foreach (var item in groupedPayments)
            {
                item.Percentage = maxPaymentAmount > 0 ? (double)(item.TotalAmount / maxPaymentAmount) * 100 : 0;
            }
            PaymentMethods = new ObservableCollection<PaymentMethodReportItem>(groupedPayments);
        }
    }
}
