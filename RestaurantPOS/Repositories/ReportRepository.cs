using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Data;

namespace RestaurantPOS.Repositories
{
    public class ReportRepository : IReportRepository
    {
        public ReportRepository()
        {
            try
            {
                EnsureViewsExist();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error ensuring views exist: {ex.Message}");
            }
        }

        private void EnsureViewsExist()
        {
            using (var context = new RestaurantPOSDbContext())
            {
                var connectionString = context.Database.GetDbConnection().ConnectionString;
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Check and create vw_DailySalesSummary
                    if (!ViewExists(conn, "vw_DailySalesSummary"))
                    {
                        string sql = @"
                            CREATE VIEW vw_DailySalesSummary AS
                            SELECT 
                                CAST(paid_at AS DATE) AS SaleDate,
                                COUNT(invoice_id) AS TotalInvoices,
                                ISNULL(SUM(subtotal), 0) AS TotalSubtotal,
                                ISNULL(SUM(discount), 0) AS TotalDiscount,
                                ISNULL(SUM(total_amount), 0) AS TotalRevenue
                            FROM invoices
                            GROUP BY CAST(paid_at AS DATE);";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Check and create vw_DailyBestSellingDishes
                    if (!ViewExists(conn, "vw_DailyBestSellingDishes"))
                    {
                        string sql = @"
                            CREATE VIEW vw_DailyBestSellingDishes AS
                            SELECT 
                                CAST(i.paid_at AS DATE) AS SaleDate,
                                d.dish_id AS DishId,
                                d.dish_name AS DishName,
                                ISNULL(SUM(oi.quantity), 0) AS TotalQuantity,
                                ISNULL(SUM(oi.quantity * oi.unit_price), 0) AS TotalRevenue
                            FROM invoices i
                            JOIN dining_sessions ds ON i.session_id = ds.session_id
                            JOIN orders o ON ds.session_id = o.session_id
                            JOIN order_items oi ON o.order_id = oi.order_id
                            JOIN dishes d ON oi.dish_id = d.dish_id
                            WHERE oi.status IN ('ready', 'served')
                            GROUP BY CAST(i.paid_at AS DATE), d.dish_id, d.dish_name;";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Check and create vw_DailyPaymentBreakdown
                    if (!ViewExists(conn, "vw_DailyPaymentBreakdown"))
                    {
                        string sql = @"
                            CREATE VIEW vw_DailyPaymentBreakdown AS
                            SELECT 
                                CAST(i.paid_at AS DATE) AS SaleDate,
                                pd.method AS PaymentMethod,
                                ISNULL(SUM(pd.amount), 0) AS TotalAmount,
                                COUNT(pd.payment_id) AS TransactionCount
                            FROM payment_details pd
                            JOIN invoices i ON pd.invoice_id = i.invoice_id
                            GROUP BY CAST(i.paid_at AS DATE), pd.method;";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        private bool ViewExists(SqlConnection conn, string viewName)
        {
            string sql = "SELECT OBJECT_ID(@viewName, 'V')";
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@viewName", viewName);
                var result = cmd.ExecuteScalar();
                return result != DBNull.Value && result != null;
            }
        }

        public List<DailySalesSummaryDto> GetDailySalesSummary(DateTime startDate, DateTime endDate)
        {
            var list = new List<DailySalesSummaryDto>();
            try
            {
                using (var context = new RestaurantPOSDbContext())
                {
                    var connectionString = context.Database.GetDbConnection().ConnectionString;
                    using (var conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string sql = @"
                            SELECT SaleDate, TotalInvoices, TotalSubtotal, TotalDiscount, TotalRevenue 
                            FROM vw_DailySalesSummary 
                            WHERE SaleDate >= @start AND SaleDate <= @end
                            ORDER BY SaleDate DESC";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@start", startDate.Date);
                            cmd.Parameters.AddWithValue("@end", endDate.Date);
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    list.Add(new DailySalesSummaryDto
                                    {
                                        SaleDate = reader.GetDateTime(0),
                                        TotalInvoices = reader.GetInt32(1),
                                        TotalSubtotal = reader.GetDecimal(2),
                                        TotalDiscount = reader.GetDecimal(3),
                                        TotalRevenue = reader.GetDecimal(4)
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetDailySalesSummary: {ex.Message}");
            }
            return list;
        }

        public List<DailyBestSellingDishDto> GetDailyBestSellingDishes(DateTime startDate, DateTime endDate)
        {
            var list = new List<DailyBestSellingDishDto>();
            try
            {
                using (var context = new RestaurantPOSDbContext())
                {
                    var connectionString = context.Database.GetDbConnection().ConnectionString;
                    using (var conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string sql = @"
                            SELECT SaleDate, DishId, DishName, TotalQuantity, TotalRevenue 
                            FROM vw_DailyBestSellingDishes 
                            WHERE SaleDate >= @start AND SaleDate <= @end
                            ORDER BY TotalQuantity DESC, TotalRevenue DESC";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@start", startDate.Date);
                            cmd.Parameters.AddWithValue("@end", endDate.Date);
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    list.Add(new DailyBestSellingDishDto
                                    {
                                        SaleDate = reader.GetDateTime(0),
                                        DishId = reader.GetInt32(1),
                                        DishName = reader.GetString(2),
                                        TotalQuantity = reader.GetInt32(3),
                                        TotalRevenue = reader.GetDecimal(4)
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetDailyBestSellingDishes: {ex.Message}");
            }
            return list;
        }

        public List<DailyPaymentBreakdownDto> GetDailyPaymentBreakdown(DateTime startDate, DateTime endDate)
        {
            var list = new List<DailyPaymentBreakdownDto>();
            try
            {
                using (var context = new RestaurantPOSDbContext())
                {
                    var connectionString = context.Database.GetDbConnection().ConnectionString;
                    using (var conn = new SqlConnection(connectionString))
                    {
                        conn.Open();
                        string sql = @"
                            SELECT SaleDate, PaymentMethod, TotalAmount, TransactionCount 
                            FROM vw_DailyPaymentBreakdown 
                            WHERE SaleDate >= @start AND SaleDate <= @end
                            ORDER BY SaleDate DESC, TotalAmount DESC";
                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@start", startDate.Date);
                            cmd.Parameters.AddWithValue("@end", endDate.Date);
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    list.Add(new DailyPaymentBreakdownDto
                                    {
                                        SaleDate = reader.GetDateTime(0),
                                        PaymentMethod = reader.GetString(1),
                                        TotalAmount = reader.GetDecimal(2),
                                        TransactionCount = reader.GetInt32(3)
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetDailyPaymentBreakdown: {ex.Message}");
            }
            return list;
        }
    }
}
