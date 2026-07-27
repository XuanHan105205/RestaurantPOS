using System.Collections.Generic;
using System.Linq;
using RestaurantPOS.Models;
using RestaurantPOS.Services;

namespace RestaurantPOS.Tests.Fakes
{
    public class FakeCustomerService : ICustomerService
    {
        public List<Customer> Customers { get; set; } = new();

        public List<Customer> GetAllCustomers()
        {
            return Customers.ToList();
        }

        public Customer GetCustomerByPhone(string phone)
        {
            return Customers.FirstOrDefault(c => c.Phone == phone)!;
        }

        public bool AddCustomer(Customer customer)
        {
            Customers.Add(customer);
            return true;
        }

        public bool UpdateCustomer(Customer customer)
        {
            return true;
        }

        public bool DeleteCustomer(int id)
        {
            Customers.RemoveAll(c => c.CustomerId == id);
            return true;
        }
    }
}
