using System.Collections.Generic;
using RestaurantPOS.Models;

namespace RestaurantPOS.Services
{
    public interface IOrderService
    {
        List<Category> GetAllCategories();
        List<Dish> GetActiveDishes();
        List<OrderItem> GetOrderItemsBySessionId(int sessionId);
        bool PlaceOrder(int sessionId, int employeeId, List<OrderItem> items);
        bool UpdateOrderItem(OrderItem item);
        bool DeleteOrderItem(int orderItemId);
    }
}
