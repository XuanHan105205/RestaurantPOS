using System.Collections.Generic;
using RestaurantPOS.Models;

namespace RestaurantPOS.Repositories
{
    public interface IOrderRepository : IBaseRepository<Order>
    {
        List<Order> GetOrdersBySessionId(int sessionId);
        List<OrderItem> GetOrderItemsByOrderId(int orderId);
        List<OrderItem> GetOrderItemsBySessionId(int sessionId);
        bool AddOrderItem(OrderItem item);
        bool UpdateOrderItem(OrderItem item);
        bool DeleteOrderItem(int orderItemId);
        List<Dish> GetActiveDishes();
        List<Category> GetAllCategories();
        bool CreateOrderWithItems(int sessionId, int employeeId, List<OrderItem> items);
    }
}
