using System.Collections.Generic;
using RestaurantPOS.Models;

namespace RestaurantPOS.Repositories
{
    public interface ITableRepository : IBaseRepository<RestaurantTable>
    {
        DiningSession GetActiveSessionByTableId(int tableId);
        DiningSession OpenSessionForTable(int tableId, int employeeId, int? customerId);
        bool UpdateTableStatus(int tableId, string status);
    }
}
