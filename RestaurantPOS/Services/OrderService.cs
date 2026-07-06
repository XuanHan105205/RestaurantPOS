using System;
using System.Collections.Generic;
using RestaurantPOS.Models;
using RestaurantPOS.Repositories;
using RestaurantPOS.Data;

namespace RestaurantPOS.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService()
        {
            _orderRepository = new OrderRepository();
        }

        public List<Category> GetAllCategories()
        {
            return _orderRepository.GetAllCategories();
        }

        public List<Dish> GetActiveDishes()
        {
            return _orderRepository.GetActiveDishes();
        }

        public List<OrderItem> GetOrderItemsBySessionId(int sessionId)
        {
            return _orderRepository.GetOrderItemsBySessionId(sessionId);
        }

        public bool PlaceOrder(int sessionId, int employeeId, List<OrderItem> items)
        {
            if (items == null || items.Count == 0) return false;

            using (var context = new RestaurantPOSDbContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var order = new Order
                        {
                            SessionId = sessionId,
                            CreatedByEmployeeId = employeeId,
                            OrderedAt = DateTime.Now
                        };
                        context.Orders.Add(order);
                        context.SaveChanges(); // Generates OrderId

                        foreach (var item in items)
                        {
                            item.OrderId = order.OrderId;
                            item.Status = "pending";
                            item.StatusUpdatedAt = DateTime.Now;
                            context.OrderItems.Add(item);
                        }

                        context.SaveChanges();
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public bool UpdateOrderItem(OrderItem item)
        {
            return _orderRepository.UpdateOrderItem(item);
        }

        public bool DeleteOrderItem(int orderItemId)
        {
            return _orderRepository.DeleteOrderItem(orderItemId);
        }
    }
}
