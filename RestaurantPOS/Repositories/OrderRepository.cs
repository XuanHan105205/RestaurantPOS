using System;
using System.Collections.Generic;
using System.Linq;
using RestaurantPOS.Models;
using RestaurantPOS.Data;

namespace RestaurantPOS.Repositories
{
    public class OrderRepository : BaseRepository<Order>, IOrderRepository
    {
        public override List<Order> GetAll()
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.Orders.ToList();
            }
        }

        public override Order GetById(int id)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.Orders.Find(id);
            }
        }

        public override bool Add(Order entity)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                context.Orders.Add(entity);
                return context.SaveChanges() > 0;
            }
        }

        public override bool Update(Order entity)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                context.Orders.Update(entity);
                return context.SaveChanges() > 0;
            }
        }

        public override bool Delete(int id)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                var order = context.Orders.Find(id);
                if (order != null)
                {
                    context.Orders.Remove(order);
                    return context.SaveChanges() > 0;
                }
                return false;
            }
        }

        public List<Order> GetOrdersBySessionId(int sessionId)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.Orders.Where(o => o.SessionId == sessionId).ToList();
            }
        }

        public List<OrderItem> GetOrderItemsByOrderId(int orderId)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.OrderItems.Where(oi => oi.OrderId == orderId).ToList();
            }
        }

        public List<OrderItem> GetOrderItemsBySessionId(int sessionId)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return (from o in context.Orders
                        join oi in context.OrderItems on o.OrderId equals oi.OrderId
                        where o.SessionId == sessionId
                        select oi).ToList();
            }
        }

        public bool AddOrderItem(OrderItem item)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                context.OrderItems.Add(item);
                return context.SaveChanges() > 0;
            }
        }

        public bool UpdateOrderItem(OrderItem item)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                context.OrderItems.Update(item);
                return context.SaveChanges() > 0;
            }
        }

        public bool DeleteOrderItem(int orderItemId)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                var item = context.OrderItems.Find(orderItemId);
                if (item != null)
                {
                    context.OrderItems.Remove(item);
                    return context.SaveChanges() > 0;
                }
                return false;
            }
        }

        public List<Dish> GetActiveDishes()
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.Dishes.Where(d => d.AvailabilityStatus == "active").ToList();
            }
        }

        public List<Category> GetAllCategories()
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.Categories.ToList();
            }
        }
    }
}
