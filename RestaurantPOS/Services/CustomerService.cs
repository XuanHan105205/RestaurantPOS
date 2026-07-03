using System.Collections.Generic;
using RestaurantPOS.Models;

namespace RestaurantPOS.Services
{
    public class CustomerService : ICustomerService
    {
        public List<Customer> GetAllCustomers()
        {
            // TODO: Hàn will implement this.
            return new List<Customer>();
        }

        public Customer GetCustomerByPhone(string phone)
        {
            // TODO: Hàn will implement this.
            return null;
        }

        public bool AddCustomer(Customer customer)
        {
            // TODO: Hàn will implement this.
            return false;
        }

        public bool UpdateCustomer(Customer customer)
        {
            // TODO: Hàn will implement this.
            return false;
        }
    }
}
