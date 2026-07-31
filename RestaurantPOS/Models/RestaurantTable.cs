namespace RestaurantPOS.Models
{
    public class RestaurantTable
    {
        public int TableId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public int? Capacity { get; set; }
        public string Status { get; set; } = "available"; // 'available', 'occupied', 'needs_cleaning', 'reserved'
        public string Area { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
