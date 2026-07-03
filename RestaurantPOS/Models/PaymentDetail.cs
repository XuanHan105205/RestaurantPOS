namespace RestaurantPOS.Models
{
    public class PaymentDetail
    {
        public int PaymentId { get; set; }
        public int InvoiceId { get; set; }
        public string Method { get; set; } // 'cash', 'bank_transfer', 'card'
        public decimal Amount { get; set; }
    }
}
