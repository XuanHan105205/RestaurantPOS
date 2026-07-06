using System;
using System.Collections.Generic;

namespace RestaurantPOS.Repositories
{
    public class DailySalesSummaryDto
    {
        public DateTime SaleDate { get; set; }
        public int TotalInvoices { get; set; }
        public decimal TotalSubtotal { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class DailyBestSellingDishDto
    {
        public DateTime SaleDate { get; set; }
        public int DishId { get; set; }
        public string DishName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class DailyPaymentBreakdownDto
    {
        public DateTime SaleDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int TransactionCount { get; set; }
    }

    public interface IReportRepository
    {
        List<DailySalesSummaryDto> GetDailySalesSummary(DateTime startDate, DateTime endDate);
        List<DailyBestSellingDishDto> GetDailyBestSellingDishes(DateTime startDate, DateTime endDate);
        List<DailyPaymentBreakdownDto> GetDailyPaymentBreakdown(DateTime startDate, DateTime endDate);
    }
}
