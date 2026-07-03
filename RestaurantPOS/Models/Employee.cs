namespace RestaurantPOS.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } // 'waiter', 'kitchen', 'cashier', 'manager'
        public string Phone { get; set; }
        public bool IsActive { get; set; }
    }
}
