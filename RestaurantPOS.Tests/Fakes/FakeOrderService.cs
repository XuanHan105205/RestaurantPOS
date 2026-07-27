using System.Collections.Generic;
using System.Linq;
using RestaurantPOS.Models;
using RestaurantPOS.Services;

namespace RestaurantPOS.Tests.Fakes
{
    public class FakeOrderService : IOrderService
    {
        public List<Category> Categories { get; set; } = new();
        public List<Dish> Dishes { get; set; } = new();
        public List<OrderItem> OrderItems { get; set; } = new();
        public bool PlaceOrderResult { get; set; } = true;

        public List<Category> GetAllCategories()
        {
            return Categories.ToList();
        }

        public List<Dish> GetActiveDishes()
        {
            return Dishes.Where(d => d.AvailabilityStatus == "active").ToList();
        }

        public List<OrderItem> GetOrderItemsBySessionId(int sessionId)
        {
            return OrderItems.ToList();
        }

        public bool PlaceOrder(int sessionId, int employeeId, List<OrderItem> items)
        {
            if (!PlaceOrderResult) return false;

            foreach (var item in items)
            {
                item.OrderItemId = OrderItems.Count + 1;
                OrderItems.Add(item);
            }
            return true;
        }

        public bool UpdateOrderItem(OrderItem item)
        {
            var existing = OrderItems.FirstOrDefault(i => i.OrderItemId == item.OrderItemId);
            if (existing != null)
            {
                existing.Quantity = item.Quantity;
                existing.Note = item.Note;
                existing.Status = item.Status;
                existing.StatusUpdatedAt = item.StatusUpdatedAt;
                return true;
            }
            return false;
        }

        public bool DeleteOrderItem(int orderItemId)
        {
            OrderItems.RemoveAll(i => i.OrderItemId == orderItemId);
            return true;
        }
    }
}
