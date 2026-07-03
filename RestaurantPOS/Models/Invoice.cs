using System;

namespace RestaurantPOS.Models
{
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public int SessionId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime PaidAt { get; set; }
        public int? CashierEmployeeId { get; set; }
    }
}
