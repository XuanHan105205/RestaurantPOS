using System;
using System.Collections.Generic;
using System.Linq;
using RestaurantPOS.Models;
using RestaurantPOS.Data;

namespace RestaurantPOS.Repositories
{
    public class TableRepository : BaseRepository<RestaurantTable>, ITableRepository
    {
        public override List<RestaurantTable> GetAll()
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.RestaurantTables.ToList();
            }
        }

        public override RestaurantTable GetById(int id)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.RestaurantTables.Find(id);
            }
        }

        public override bool Add(RestaurantTable entity)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                context.RestaurantTables.Add(entity);
                return context.SaveChanges() > 0;
            }
        }

        public override bool Update(RestaurantTable entity)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                context.RestaurantTables.Update(entity);
                return context.SaveChanges() > 0;
            }
        }

        public override bool Delete(int id)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                var table = context.RestaurantTables.Find(id);
                if (table != null)
                {
                    context.RestaurantTables.Remove(table);
                    return context.SaveChanges() > 0;
                }
                return false;
            }
        }

        public DiningSession GetActiveSessionByTableId(int tableId)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return (from ts in context.TableSessions
                        join ds in context.DiningSessions on ts.SessionId equals ds.SessionId
                        where ts.TableId == tableId && ds.Status == "open"
                        select ds).FirstOrDefault();
            }
        }

        public DiningSession OpenSessionForTable(int tableId, int employeeId, int? customerId)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var session = new DiningSession
                        {
                            OpenedAt = DateTime.Now,
                            OpenedByEmployeeId = employeeId,
                            CustomerId = customerId,
                            Status = "open"
                        };
                        context.DiningSessions.Add(session);
                        context.SaveChanges();

                        var tableSession = new TableSession
                        {
                            TableId = tableId,
                            SessionId = session.SessionId
                        };
                        context.TableSessions.Add(tableSession);

                        var table = context.RestaurantTables.Find(tableId);
                        if (table != null)
                        {
                            table.Status = "occupied";
                            context.RestaurantTables.Update(table);
                        }

                        context.SaveChanges();
                        transaction.Commit();
                        return session;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public bool UpdateTableStatus(int tableId, string status)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                var table = context.RestaurantTables.Find(tableId);
                if (table != null)
                {
                    table.Status = status;
                    context.RestaurantTables.Update(table);
                    return context.SaveChanges() > 0;
                }
                return false;
            }
        }
    }
}
