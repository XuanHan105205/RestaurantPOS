using System;

namespace RestaurantPOS.Models
{
    public class StockMovement
    {
        public int MovementId { get; set; }
        public int IngredientId { get; set; }
        public string MovementType { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal QuantityBefore { get; set; }
        public decimal QuantityAfter { get; set; }
        public string? Reason { get; set; }
        public int? ReferenceId { get; set; }
        public int? EmployeeId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
