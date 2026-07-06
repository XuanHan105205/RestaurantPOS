using System.Collections.Generic;
using RestaurantPOS.Models;
using RestaurantPOS.Repositories;

namespace RestaurantPOS.Services
{
    public class TableService : ITableService
    {
        private readonly ITableRepository _tableRepository;

        public TableService()
        {
            _tableRepository = new TableRepository();
        }

        public List<RestaurantTable> GetAllTables()
        {
            return _tableRepository.GetAll();
        }

        public DiningSession GetActiveSessionByTableId(int tableId)
        {
            return _tableRepository.GetActiveSessionByTableId(tableId);
        }

        public DiningSession OpenSessionForTable(int tableId, int employeeId, int? customerId)
        {
            return _tableRepository.OpenSessionForTable(tableId, employeeId, customerId);
        }

        public bool UpdateTableStatus(int tableId, string status)
        {
            return _tableRepository.UpdateTableStatus(tableId, status);
        }
    }
}
