using System;
using System.Collections.Generic;
using RestaurantPOS.Repositories;

namespace RestaurantPOS.Services
{
    public interface IReportService
    {
        List<DailySalesSummaryDto> GetDailySalesSummary(DateTime startDate, DateTime endDate);
        List<DailyBestSellingDishDto> GetDailyBestSellingDishes(DateTime startDate, DateTime endDate);
        List<DailyPaymentBreakdownDto> GetDailyPaymentBreakdown(DateTime startDate, DateTime endDate);
    }
}
