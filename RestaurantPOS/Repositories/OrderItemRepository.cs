using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Data;
using RestaurantPOS.Models;

namespace RestaurantPOS.Repositories
{
    public class OrderItemRepository : BaseRepository<OrderItem>, IOrderItemRepository
    {
        public override List<OrderItem> GetAll()
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.OrderItems.ToList();
            }
        }

        public override OrderItem GetById(int id)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.OrderItems.Find(id);
            }
        }

        public override bool Add(OrderItem entity)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                context.OrderItems.Add(entity);
                return context.SaveChanges() > 0;
            }
        }

        public override bool Update(OrderItem entity)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                context.OrderItems.Update(entity);
                return context.SaveChanges() > 0;
            }
        }

        public override bool Delete(int id)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                var item = context.OrderItems.Find(id);
                if (item != null)
                {
                    context.OrderItems.Remove(item);
                    return context.SaveChanges() > 0;
                }
                return false;
            }
        }

        public List<KitchenOrderItemDto> GetActiveKitchenItems()
        {
            using (var context = new RestaurantPOSDbContext())
            {
                var query = from oi in context.OrderItems
                            join o in context.Orders on oi.OrderId equals o.OrderId
                            join d in context.Dishes on oi.DishId equals d.DishId
                            where oi.Status == "pending" || oi.Status == "cooking" || oi.Status == "ready"
                            select new
                            {
                                oi.OrderItemId,
                                oi.OrderId,
                                d.DishName,
                                oi.Quantity,
                                oi.Note,
                                oi.Status,
                                oi.StatusUpdatedAt,
                                o.OrderedAt,
                                o.SessionId
                            };

                var items = query.ToList();
                if (!items.Any()) return new List<KitchenOrderItemDto>();

                var sessionIds = items.Select(x => x.SessionId).Distinct().ToList();
                var tableSessions = (from ts in context.TableSessions
                                    join t in context.RestaurantTables on ts.TableId equals t.TableId
                                    where sessionIds.Contains(ts.SessionId)
                                    select new { ts.SessionId, t.TableName }).ToList();

                var tableMap = tableSessions
                    .GroupBy(x => x.SessionId)
                    .ToDictionary(
                        g => g.Key,
                        g => string.Join(", ", g.Select(x => x.TableName))
                    );

                return items.Select(x => new KitchenOrderItemDto
                {
                    OrderItemId = x.OrderItemId,
                    OrderId = x.OrderId,
                    DishName = x.DishName,
                    Quantity = x.Quantity,
                    Note = x.Note,
                    Status = x.Status,
                    StatusUpdatedAt = x.StatusUpdatedAt,
                    OrderedAt = x.OrderedAt,
                    TableName = tableMap.ContainsKey(x.SessionId) ? tableMap[x.SessionId] : "Không xác định"
                }).OrderBy(x => x.OrderedAt).ToList();
            }
        }

        public List<KitchenOrderItemDto> GetServedKitchenItemsToday()
        {
            using (var context = new RestaurantPOSDbContext())
            {
                DateTime today = DateTime.Today;
                var query = from oi in context.OrderItems
                            join o in context.Orders on oi.OrderId equals o.OrderId
                            join d in context.Dishes on oi.DishId equals d.DishId
                            where oi.Status == "served" && oi.StatusUpdatedAt >= today
                            select new
                            {
                                oi.OrderItemId,
                                oi.OrderId,
                                d.DishName,
                                oi.Quantity,
                                oi.Note,
                                oi.Status,
                                oi.StatusUpdatedAt,
                                o.OrderedAt,
                                o.SessionId
                            };

                var items = query.ToList();
                if (!items.Any()) return new List<KitchenOrderItemDto>();

                var sessionIds = items.Select(x => x.SessionId).Distinct().ToList();
                var tableSessions = (from ts in context.TableSessions
                                    join t in context.RestaurantTables on ts.TableId equals t.TableId
                                    where sessionIds.Contains(ts.SessionId)
                                    select new { ts.SessionId, t.TableName }).ToList();

                var tableMap = tableSessions
                    .GroupBy(x => x.SessionId)
                    .ToDictionary(
                        g => g.Key,
                        g => string.Join(", ", g.Select(x => x.TableName))
                    );

                return items.Select(x => new KitchenOrderItemDto
                {
                    OrderItemId = x.OrderItemId,
                    OrderId = x.OrderId,
                    DishName = x.DishName,
                    Quantity = x.Quantity,
                    Note = x.Note,
                    Status = x.Status,
                    StatusUpdatedAt = x.StatusUpdatedAt,
                    OrderedAt = x.OrderedAt,
                    TableName = tableMap.ContainsKey(x.SessionId) ? tableMap[x.SessionId] : "Không xác định"
                }).OrderByDescending(x => x.StatusUpdatedAt).ToList();
            }
        }
    }
}
