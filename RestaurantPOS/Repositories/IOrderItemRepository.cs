using System;
using System.Collections.Generic;
using RestaurantPOS.Models;

namespace RestaurantPOS.Repositories
{
    public class KitchenOrderItemDto
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public string DishName { get; set; }
        public int Quantity { get; set; }
        public string Note { get; set; }
        public string Status { get; set; } // 'pending', 'cooking', 'ready', 'served', 'cancelled'
        public DateTime? StatusUpdatedAt { get; set; }
        public DateTime OrderedAt { get; set; }
        public string TableName { get; set; }
        public int MinutesSinceOrdered => (int)(DateTime.Now - OrderedAt).TotalMinutes;
        public string DelayLevel
        {
            get
            {
                int min = MinutesSinceOrdered;
                if (min >= 20) return "High";
                if (min >= 10) return "Medium";
                return "Normal";
            }
        }
    }

    public interface IOrderItemRepository : IBaseRepository<OrderItem>
    {
        List<KitchenOrderItemDto> GetActiveKitchenItems();
        List<KitchenOrderItemDto> GetServedKitchenItemsToday();
    }
}
