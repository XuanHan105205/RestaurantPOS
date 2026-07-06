using System;
using System.Collections.Generic;
using RestaurantPOS.Models;
using RestaurantPOS.Repositories;

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
            return _orderRepository.CreateOrderWithItems(sessionId, employeeId, items);
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
