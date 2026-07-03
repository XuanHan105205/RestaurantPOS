using System;

namespace RestaurantPOS.Models
{
    public class DiningSession
    {
        public int SessionId { get; set; }
        public DateTime OpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public int OpenedByEmployeeId { get; set; }
        public int? CustomerId { get; set; }
        public string Status { get; set; } // 'open', 'closed'
    }
}
