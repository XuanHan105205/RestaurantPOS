using System;

namespace RestaurantPOS.Models
{
    public class Reservation
    {
        public int ReservationId { get; set; }
        public int CustomerId { get; set; }
        public int TableId { get; set; }
        public DateTime ReservationTime { get; set; }
        public int GuestCount { get; set; }
        public string Status { get; set; } = "confirmed";
        public string? Note { get; set; }
        public int CreatedByEmployeeId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
