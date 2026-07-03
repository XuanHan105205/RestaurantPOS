using System;

namespace RestaurantPOS.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int SessionId { get; set; }
        public int CreatedByEmployeeId { get; set; }
        public DateTime OrderedAt { get; set; }
    }
}
