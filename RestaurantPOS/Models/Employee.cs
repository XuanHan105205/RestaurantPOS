namespace RestaurantPOS.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // 'waiter', 'kitchen', 'cashier', 'manager'
        public string Phone { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
