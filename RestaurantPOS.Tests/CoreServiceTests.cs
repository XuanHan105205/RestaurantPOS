using System.Collections.Generic;
using RestaurantPOS.Models;
using RestaurantPOS.Repositories;
using RestaurantPOS.Services;
using Xunit;

namespace RestaurantPOS.Tests
{
    public class FakeAuthEmployeeRepository : IEmployeeRepository
    {
        public Employee Employee { get; set; }

        public Employee GetByUsername(string username)
        {
            return Employee != null && Employee.Username == username ? Employee : null;
        }

        public List<Employee> GetAll() => new List<Employee>();
        public Employee GetById(int id) => null;
        public bool Add(Employee entity) => true;
        public bool Update(Employee entity) => true;
        public bool Delete(int id) => true;
    }

    public class FakeCustomerRepository : ICustomerRepository
    {
        public List<Customer> Customers { get; } = new List<Customer>();

        public List<Customer> GetAll() => new List<Customer>(Customers);
        public Customer GetById(int id) => Customers.Find(c => c.CustomerId == id);
        public Customer GetByPhone(string phone) => Customers.Find(c => c.Phone == phone);

        public bool Add(Customer entity)
        {
            entity.CustomerId = Customers.Count + 1;
            Customers.Add(entity);
            return true;
        }

        public bool Update(Customer entity) => true;
        public bool Delete(int id) => Customers.RemoveAll(c => c.CustomerId == id) > 0;
    }

    public class CoreServiceTests
    {
        [Fact]
        public void Login_CorrectAccount_ReturnsTrue()
        {
            var repository = new FakeAuthEmployeeRepository
            {
                Employee = new Employee
                {
                    Username = "manager",
                    PasswordHash = "123456",
                    IsActive = true
                }
            };
            var service = new AuthService(repository);

            bool result = service.Login("manager", "123456");

            Assert.True(result);
            Assert.NotNull(service.CurrentUser);
        }

        [Fact]
        public void Login_InactiveAccount_ReturnsFalse()
        {
            var repository = new FakeAuthEmployeeRepository
            {
                Employee = new Employee
                {
                    Username = "manager",
                    PasswordHash = "123456",
                    IsActive = false
                }
            };
            var service = new AuthService(repository);

            Assert.False(service.Login("manager", "123456"));
        }

        [Fact]
        public void AddCustomer_DuplicatePhone_ReturnsFalse()
        {
            var repository = new FakeCustomerRepository();
            repository.Customers.Add(new Customer { CustomerId = 1, Phone = "0901234567" });
            var service = new CustomerService(repository);

            bool result = service.AddCustomer(new Customer
            {
                FullName = "Khach moi",
                Phone = "0901234567",
                LoyaltyPoints = 0
            });

            Assert.False(result);
        }

        [Fact]
        public void AddCustomer_EnoughPoints_SetsVipTier()
        {
            var repository = new FakeCustomerRepository();
            var service = new CustomerService(repository);
            var customer = new Customer
            {
                FullName = "Khach VIP",
                Phone = "0901234567",
                LoyaltyPoints = 600
            };

            bool result = service.AddCustomer(customer);

            Assert.True(result);
            Assert.Equal("vip", customer.MembershipTier);
        }

        [Fact]
        public void AddCustomer_NegativePoints_ReturnsFalse()
        {
            var service = new CustomerService(new FakeCustomerRepository());

            bool result = service.AddCustomer(new Customer
            {
                FullName = "Khach hang",
                Phone = "0901234567",
                LoyaltyPoints = -1
            });

            Assert.False(result);
        }
    }
}
