using RestaurantPOS.Models;
using RestaurantPOS.Repositories;

namespace RestaurantPOS.Services
{
    public class AuthService : IAuthService
    {
        private static AuthService? _instance;
        public static AuthService Instance => _instance ??= new AuthService();

        public Employee? CurrentUser { get; private set; }

        private readonly IEmployeeRepository _employeeRepository;

        private AuthService()
            : this(new EmployeeRepository())
        {
        }

        // Constructor này giúp kiểm thử mà không cần kết nối database thật.
        public AuthService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public bool Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            {
                return false;
            }

            var employee = _employeeRepository.GetByUsername(username.Trim());
            if (employee != null && employee.IsActive && employee.PasswordHash == password)
            {
                CurrentUser = employee;
                return true;
            }
            return false;
        }

        public void Logout()
        {
            CurrentUser = null;
        }
    }
}
