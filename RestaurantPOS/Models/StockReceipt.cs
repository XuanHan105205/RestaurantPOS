using System;

namespace RestaurantPOS.Models
{
    public class StockReceipt
    {
        public int ReceiptId { get; set; }
        public int IngredientId { get; set; }
        public decimal Quantity { get; set; }
        public decimal? UnitCost { get; set; }
        public DateTime ReceivedAt { get; set; }
        public int? ReceivedByEmployeeId { get; set; }
        public string Supplier { get; set; }
    }
}
