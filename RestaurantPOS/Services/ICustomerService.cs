using System.Collections.Generic;
using RestaurantPOS.Models;

namespace RestaurantPOS.Services
{
    public interface ICustomerService
    {
        List<Customer> GetAllCustomers();
        Customer GetCustomerByPhone(string phone);
        bool AddCustomer(Customer customer);
        bool UpdateCustomer(Customer customer);
    }
}
