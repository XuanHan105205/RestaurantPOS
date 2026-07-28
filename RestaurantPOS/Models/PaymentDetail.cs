namespace RestaurantPOS.Models
{
    public class PaymentDetail
    {
        public int PaymentId { get; set; }
        public int InvoiceId { get; set; }
        public string Method { get; set; } = string.Empty; // 'cash', 'bank_transfer', 'card'
        public decimal Amount { get; set; }
    }
}
