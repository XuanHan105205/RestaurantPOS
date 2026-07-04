using RestaurantPOS.Models;
using RestaurantPOS.Repositories; // <-- Thêm dòng này

namespace RestaurantPOS.Services
{
    public class AuthService : IAuthService
    {
        private static AuthService _instance;
        public static AuthService Instance => _instance ??= new AuthService();

        public Employee CurrentUser { get; private set; }

        private readonly IEmployeeRepository _employeeRepository; // <-- Khai báo repo

        // --- Cập nhật Constructor để khởi tạo repository ---
        private AuthService()
        {
            _employeeRepository = new EmployeeRepository();
        }

        // --- COPY VÀ THAY THẾ HÀM Login CŨ BẰNG ĐOẠN DƯỚI ĐÂY ---
        public bool Login(string username, string password)
        {
            // 1. Lấy thông tin nhân viên theo Username dưới Database
            var employee = _employeeRepository.GetByUsername(username);

            // 2. So sánh mật khẩu và trạng thái hoạt động
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
