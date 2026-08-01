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

        public override OrderItem? GetById(int id)
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

        public bool TryMarkReadyAndDeductStock(int orderItemId, int? employeeId)
        {
            using var context = new RestaurantPOSDbContext();
            using var transaction = context.Database.BeginTransaction();
            var item = context.OrderItems.Find(orderItemId);
            if (item == null || item.Status != "cooking") return false;
            var recipes = context.Recipes.Where(r => r.DishId == item.DishId).ToList();
            if (recipes.Count == 0) return false;
            foreach (var recipe in recipes)
            {
                var ingredient = context.Ingredients.Find(recipe.IngredientId);
                decimal required = recipe.QuantityPerServing * item.Quantity;
                if (ingredient == null || ingredient.StockQuantity < required) return false;
            }
            foreach (var recipe in recipes)
            {
                var ingredient = context.Ingredients.Find(recipe.IngredientId)!;
                decimal before = ingredient.StockQuantity;
                decimal required = recipe.QuantityPerServing * item.Quantity;
                ingredient.StockQuantity -= required;
                context.StockMovements.Add(new StockMovement { IngredientId = ingredient.IngredientId,
                    MovementType = "sale", Quantity = required, QuantityBefore = before,
                    QuantityAfter = ingredient.StockQuantity, ReferenceId = item.OrderItemId,
                    EmployeeId = employeeId, Reason = $"Hoàn thành món #{item.OrderItemId}", CreatedAt = DateTime.Now });
            }
            item.Status = "ready";
            item.StatusUpdatedAt = DateTime.Now;
            context.SaveChanges();
            transaction.Commit();
            return true;
        }

        public List<string> GetMissingIngredientsForOrderItem(int orderItemId)
        {
            var missing = new List<string>();
            using var context = new RestaurantPOSDbContext();
            var item = context.OrderItems.Find(orderItemId);
            if (item == null) return missing;

            var recipes = context.Recipes.Where(r => r.DishId == item.DishId).ToList();
            foreach (var recipe in recipes)
            {
                var ingredient = context.Ingredients.Find(recipe.IngredientId);
                decimal required = recipe.QuantityPerServing * item.Quantity;
                if (ingredient == null)
                {
                    missing.Add($"Nguyên liệu ID #{recipe.IngredientId} (Không tìm thấy)");
                }
                else if (ingredient.StockQuantity < required)
                {
                    missing.Add($"{ingredient.IngredientName} (Tồn kho: {ingredient.StockQuantity:0.##} {ingredient.Unit}, cần: {required:0.##} {ingredient.Unit})");
                }
            }
            return missing;
        }

        public bool CancelOrderItem(int orderItemId, string reason, int? employeeId)
        {
            using var context = new RestaurantPOSDbContext();
            var item = context.OrderItems.Find(orderItemId);
            if (item == null || item.Status == "served" || item.Status == "cancelled") return false;

            item.Status = "cancelled";
            item.StatusUpdatedAt = DateTime.Now;
            item.Note = string.IsNullOrEmpty(item.Note)
                ? $"[HỦY BẾP]: {reason}"
                : $"{item.Note} | [HỦY BẾP]: {reason}";

            context.AuditLogs.Add(new AuditLog
            {
                EmployeeId = employeeId,
                Action = "CANCEL_ORDER_ITEM",
                EntityType = "order_items",
                EntityId = item.OrderItemId,
                Description = $"Bếp hủy món #{item.OrderItemId} ({item.DishId}). Lý do: {reason}",
                CreatedAt = DateTime.Now
            });

            return context.SaveChanges() > 0;
        }
    }
}
