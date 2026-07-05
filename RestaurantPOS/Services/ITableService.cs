using System.Collections.Generic;
using RestaurantPOS.Models;

namespace RestaurantPOS.Services
{
    public interface ITableService
    {
        List<RestaurantTable> GetAllTables();
        DiningSession GetActiveSessionByTableId(int tableId);
        DiningSession OpenSessionForTable(int tableId, int employeeId, int? customerId);
        bool UpdateTableStatus(int tableId, string status);
    }
}
