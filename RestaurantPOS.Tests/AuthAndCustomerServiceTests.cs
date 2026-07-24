using System;
using System.Collections.Generic;
using RestaurantPOS.Models;
using RestaurantPOS.Repositories;
using RestaurantPOS.Services;
using Xunit;

namespace RestaurantPOS.Tests
{
    public class FakeEmployeeRepository : IEmployeeRepository
    {
        private readonly List<Employee> _employees = new List<Employee>
        {
            new Employee
            {
                EmployeeId = 1,
                FullName = "Lê Xuân Hàn",
                Username = "manager",
                PasswordHash = PasswordHasher.HashPassword("123456"),
                Role = "manager",
                IsActive = true
            },
            new Employee
            {
                EmployeeId = 2,
                FullName = "Nhân viên cũ",
                Username = "waiter",
                PasswordHash = "123456", // Seed plaintext fallback
                Role = "waiter",
                IsActive = true
            },
            new Employee
            {
                EmployeeId = 3,
                FullName = "Nhân viên bị khóa",
                Username = "disabled",
                PasswordHash = "123456",
                Role = "waiter",
                IsActive = false
            }
        };

        public List<Employee> GetAll() => new List<Employee>(_employees);

        public Employee? GetById(int id) => _employees.Find(e => e.EmployeeId == id);

        public Employee? GetByUsername(string username) => _employees.Find(e => e.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        public bool Add(Employee entity)
        {
            _employees.Add(entity);
            return true;
        }

        public bool Update(Employee entity) => true;

        public bool Delete(int id) => true;
    }

    public class FakeCustomerRepository : ICustomerRepository
    {
        private readonly List<Customer> _customers = new List<Customer>();

        public List<Customer> GetAll() => new List<Customer>(_customers);

        public Customer? GetById(int id) => _customers.Find(c => c.CustomerId == id);

        public Customer? GetByPhone(string phone) => _customers.Find(c => c.Phone == phone);

        public bool Add(Customer entity)
        {
            if (entity.CustomerId <= 0) entity.CustomerId = _customers.Count + 1;
            _customers.Add(entity);
            return true;
        }

        public bool Update(Customer entity)
        {
            var idx = _customers.FindIndex(c => c.CustomerId == entity.CustomerId);
            if (idx >= 0)
            {
                _customers[idx] = entity;
                return true;
            }
            return false;
        }

        public bool Delete(int id)
        {
            return _customers.RemoveAll(c => c.CustomerId == id) > 0;
        }
    }

    public class AuthAndCustomerServiceTests
    {
        [Fact]
        public void PasswordHasher_HashAndVerify_ReturnsTrue()
        {
            string password = "MySecretPassword123";
            string hash = PasswordHasher.HashPassword(password);

            bool isValid = PasswordHasher.VerifyPassword(password, hash);

            Assert.True(isValid);
        }

        [Fact]
        public void PasswordHasher_FallbackVerification_ReturnsTrue()
        {
            string plainTextSeed = "123456";
            bool isValid = PasswordHasher.VerifyPassword("123456", plainTextSeed);

            Assert.True(isValid);
        }

        [Fact]
        public void AuthService_ValidLogin_ReturnsTrue()
        {
            var repo = new FakeEmployeeRepository();
            var authService = new AuthService(repo);

            bool success = authService.Login("manager", "123456", out string error);

            Assert.True(success);
            Assert.Empty(error);
            Assert.NotNull(authService.CurrentUser);
            Assert.Equal("Lê Xuân Hàn", authService.CurrentUser.FullName);
        }

        [Fact]
        public void AuthService_EmptyUsername_ReturnsFalse()
        {
            var repo = new FakeEmployeeRepository();
            var authService = new AuthService(repo);

            bool success = authService.Login("", "123456", out string error);

            Assert.False(success);
            Assert.Equal("Tên đăng nhập không được để trống.", error);
        }

        [Fact]
        public void AuthService_DisabledAccount_ReturnsFalse()
        {
            var repo = new FakeEmployeeRepository();
            var authService = new AuthService(repo);

            bool success = authService.Login("disabled", "123456", out string error);

            Assert.False(success);
            Assert.Contains("khóa", error);
        }

        [Fact]
        public void CustomerService_AddCustomer_ValidPhone_ReturnsTrue()
        {
            var repo = new FakeCustomerRepository();
            var customerService = new CustomerService(repo);
            var customer = new Customer
            {
                FullName = "Nguyễn Văn A",
                Phone = "0987654321",
                LoyaltyPoints = 600
            };

            bool success = customerService.AddCustomer(customer);

            Assert.True(success);
            Assert.Equal("vip", customer.MembershipTier); // Tự động thăng hạng VIP khi >= 500 điểm
        }

        [Fact]
        public void CustomerService_AddCustomer_InvalidPhone_ReturnsFalse()
        {
            var repo = new FakeCustomerRepository();
            var customerService = new CustomerService(repo);
            var customer = new Customer
            {
                FullName = "Trần Văn B",
                Phone = "123" // Không đủ 10 chữ số
            };

            bool success = customerService.AddCustomer(customer);

            Assert.False(success);
        }

        [Fact]
        public void CustomerService_DiscountCalculation_ReturnsCorrectRate()
        {
            var repo = new FakeCustomerRepository();
            var customerService = new CustomerService(repo);

            var vipGoldCustomer = new Customer { FullName = "VIP Gold", Phone = "0911111111", MembershipTier = "vip_gold" };
            var vipCustomer = new Customer { FullName = "VIP", Phone = "0922222222", MembershipTier = "vip" };
            var regularCustomer = new Customer { FullName = "Regular", Phone = "0933333333", MembershipTier = "regular" };

            Assert.Equal(0.10, customerService.GetDiscountRateByCustomer(vipGoldCustomer));
            Assert.Equal(0.05, customerService.GetDiscountRateByCustomer(vipCustomer));
            Assert.Equal(0.00, customerService.GetDiscountRateByCustomer(regularCustomer));
        }
    }
}
