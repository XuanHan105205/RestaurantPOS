using System;

namespace RestaurantPOS.Models
{
    public class AuditLog
    {
        public int AuditLogId { get; set; }
        public int? EmployeeId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public int? EntityId { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
