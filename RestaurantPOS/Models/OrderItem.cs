using System;

namespace RestaurantPOS.Models
{
    public class OrderItem
    {
        public int OrderItemId { get; set; }
        public int OrderId { get; set; }
        public int DishId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Status { get; set; } = "pending"; // 'pending', 'cooking', 'ready', 'served', 'cancelled'
        public string Note { get; set; } = string.Empty;
        public DateTime? StatusUpdatedAt { get; set; }
    }
}
