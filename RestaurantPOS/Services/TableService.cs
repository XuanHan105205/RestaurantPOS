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

        public DiningSession? GetActiveSessionByTableId(int tableId)
        {
            return _tableRepository.GetActiveSessionByTableId(tableId);
        }

        public DiningSession OpenSessionForTable(int tableId, int employeeId, int? customerId)
        {
            var session = _tableRepository.OpenSessionForTable(tableId, employeeId, customerId);
            AuditTrail.Record("open_session", "dining_session", session.SessionId,
                $"Mở bàn #{tableId}; khách hàng #{customerId?.ToString() ?? "vãng lai"}.", employeeId);
            return session;
        }

        public bool UpdateTableStatus(int tableId, string status)
        {
            bool success = _tableRepository.UpdateTableStatus(tableId, status);
            if (success)
                AuditTrail.Record("update_status", "table", tableId, $"Chuyển trạng thái bàn thành {status}.");
            return success;
        }

        public List<int> GetTableIdsBySessionId(int sessionId)
        {
            return _tableRepository.GetTableIdsBySessionId(sessionId);
        }
    }
}
