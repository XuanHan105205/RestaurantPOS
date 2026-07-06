using System;
using System.Collections.Generic;
using RestaurantPOS.Repositories;

namespace RestaurantPOS.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

        public ReportService()
        {
            _reportRepository = new ReportRepository();
        }

        public List<DailySalesSummaryDto> GetDailySalesSummary(DateTime startDate, DateTime endDate)
        {
            return _reportRepository.GetDailySalesSummary(startDate, endDate);
        }

        public List<DailyBestSellingDishDto> GetDailyBestSellingDishes(DateTime startDate, DateTime endDate)
        {
            return _reportRepository.GetDailyBestSellingDishes(startDate, endDate);
        }

        public List<DailyPaymentBreakdownDto> GetDailyPaymentBreakdown(DateTime startDate, DateTime endDate)
        {
            return _reportRepository.GetDailyPaymentBreakdown(startDate, endDate);
        }
    }
}
