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
        private readonly Action<int?, string, string, int?, string>? _writeAudit;

        private AuthService()
            : this(
                new EmployeeRepository(),
                (employeeId, action, entityType, entityId, description) =>
                    new AcademicManagementRepository().AddAudit(employeeId, action, entityType, entityId, description))
        {
        }

        // Constructor này giúp kiểm thử mà không cần kết nối database thật.
        public AuthService(IEmployeeRepository employeeRepository)
            : this(employeeRepository, null)
        {
        }

        private AuthService(
            IEmployeeRepository employeeRepository,
            Action<int?, string, string, int?, string>? writeAudit)
        {
            _employeeRepository = employeeRepository;
            _writeAudit = writeAudit;
        }

        public bool Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrEmpty(password))
            {
                return false;
            }

            var employee = _employeeRepository.GetByUsername(username.Trim());
            if (employee != null && employee.IsActive && PasswordSecurity.Verify(password, employee.PasswordHash))
            {
                if (PasswordSecurity.IsLegacy(employee.PasswordHash))
                {
                    employee.PasswordHash = PasswordSecurity.Hash(password);
                    _employeeRepository.Update(employee);
                }
                CurrentUser = employee;
                TryWriteAudit(employee.EmployeeId, "login", "authentication", employee.EmployeeId,
                    $"Đăng nhập tài khoản {employee.Username} ({employee.Role}).");
                return true;
            }
            return false;
        }

        public void Logout()
        {
            var employee = CurrentUser;
            if (employee != null)
            {
                TryWriteAudit(employee.EmployeeId, "logout", "authentication", employee.EmployeeId,
                    $"Đăng xuất tài khoản {employee.Username} ({employee.Role}).");
            }
            CurrentUser = null;
        }

        private void TryWriteAudit(int? employeeId, string action, string entityType, int? entityId, string description)
        {
            try
            {
                _writeAudit?.Invoke(employeeId, action, entityType, entityId, description);
            }
            catch
            {
                // Nhật ký là dữ liệu bổ trợ, không được làm gián đoạn đăng nhập/đăng xuất.
            }
        }
    }
}
