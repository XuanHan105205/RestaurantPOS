namespace RestaurantPOS.Models
{
    public class RestaurantTable
    {
        public int TableId { get; set; }
        public string TableName { get; set; }
        public int? Capacity { get; set; }
        public string Status { get; set; } // 'available', 'occupied', 'needs_cleaning', 'reserved'
        public string Area { get; set; }
    }
}
