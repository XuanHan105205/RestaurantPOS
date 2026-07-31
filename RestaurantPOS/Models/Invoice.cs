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
        public string InvoiceNumber { get; set; } = string.Empty;
        public string Status { get; set; } = "paid";
        public DateTime? CancelledAt { get; set; }
        public int? CancelledByEmployeeId { get; set; }
        public string? CancellationReason { get; set; }
    }
}
