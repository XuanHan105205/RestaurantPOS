using System;
using System.Collections.Generic;
using System.Linq;
using RestaurantPOS.Models;
using RestaurantPOS.Services;

namespace RestaurantPOS.Tests.Fakes
{
    public class FakeTableService : ITableService
    {
        public List<RestaurantTable> Tables { get; set; } = new();
        public List<DiningSession> Sessions { get; set; } = new();
        public List<TableSession> TableSessions { get; set; } = new();
        public bool ShouldThrowOnOpenSession { get; set; } = false;

        public List<RestaurantTable> GetAllTables()
        {
            return Tables.ToList();
        }

        public DiningSession GetActiveSessionByTableId(int tableId)
        {
            var ts = TableSessions.FirstOrDefault(t => t.TableId == tableId);
            if (ts == null) return null!;
            return Sessions.FirstOrDefault(s => s.SessionId == ts.SessionId && s.Status == "open")!;
        }

        public DiningSession OpenSessionForTable(int tableId, int employeeId, int? customerId)
        {
            if (ShouldThrowOnOpenSession)
            {
                throw new InvalidOperationException("Bàn đang được xử lý bởi người khác");
            }

            var table = Tables.FirstOrDefault(t => t.TableId == tableId);
            if (table != null)
            {
                table.Status = "occupied";
            }

            var session = new DiningSession
            {
                SessionId = Sessions.Count + 1,
                OpenedByEmployeeId = employeeId,
                CustomerId = customerId,
                Status = "open",
                OpenedAt = DateTime.Now
            };

            Sessions.Add(session);
            TableSessions.Add(new TableSession { TableId = tableId, SessionId = session.SessionId });
            return session;
        }

        public bool UpdateTableStatus(int tableId, string status)
        {
            var table = Tables.FirstOrDefault(t => t.TableId == tableId);
            if (table != null)
            {
                table.Status = status;
                return true;
            }
            return false;
        }
    }
}
