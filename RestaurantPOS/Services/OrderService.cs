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
            bool success = _orderRepository.CreateOrderWithItems(sessionId, employeeId, items);
            if (success)
                AuditTrail.Record("place_order", "dining_session", sessionId,
                    $"Gọi {items.Count} món, tổng số lượng {items.Sum(item => item.Quantity)}.", employeeId);
            return success;
        }

        public bool UpdateOrderItem(OrderItem item)
        {
            bool success = _orderRepository.UpdateOrderItem(item);
            if (success)
                AuditTrail.Record("update_order_item", "order_item", item.OrderItemId,
                    $"Cập nhật số lượng thành {item.Quantity}.");
            return success;
        }

        public bool DeleteOrderItem(int orderItemId)
        {
            bool success = _orderRepository.DeleteOrderItem(orderItemId);
            if (success)
                AuditTrail.Record("cancel_order_item", "order_item", orderItemId, "Hủy món chưa chế biến.");
            return success;
        }
    }
}
