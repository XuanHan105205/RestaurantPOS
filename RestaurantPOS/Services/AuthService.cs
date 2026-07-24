using System;
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

        public AuthService()
        {
            _employeeRepository = new EmployeeRepository();
        }

        public AuthService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public bool Login(string username, string password)
        {
            return Login(username, password, out _);
        }

        public bool Login(string username, string password, out string errorMessage)
        {
            errorMessage = string.Empty;
            CurrentUser = null;

            if (string.IsNullOrWhiteSpace(username))
            {
                errorMessage = "Tên đăng nhập không được để trống.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                errorMessage = "Mật khẩu không được để trống.";
                return false;
            }

            var employee = _employeeRepository.GetByUsername(username.Trim());
            if (employee == null)
            {
                errorMessage = "Tài khoản không tồn tại trên hệ thống.";
                return false;
            }

            if (!employee.IsActive)
            {
                errorMessage = "Tài khoản của bạn đã bị khóa hoặc ngừng hoạt động.";
                return false;
            }

            if (!PasswordHasher.VerifyPassword(password, employee.PasswordHash))
            {
                errorMessage = "Mật khẩu không chính xác.";
                return false;
            }

            CurrentUser = employee;
            return true;
        }

        public void Logout()
        {
            CurrentUser = null;
        }
    }
}
