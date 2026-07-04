using System.Collections.Generic;
using RestaurantPOS.Models;
using RestaurantPOS.Repositories;

namespace RestaurantPOS.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService()
        {
            _customerRepository = new CustomerRepository();
        }

        public List<Customer> GetAllCustomers()
        {
            return _customerRepository.GetAll();
        }

        public Customer GetCustomerByPhone(string phone)
        {
            return _customerRepository.GetByPhone(phone);
        }

        public bool AddCustomer(Customer customer)
        {
            if (string.IsNullOrEmpty(customer.MembershipTier))
            {
                customer.MembershipTier = "regular";
            }
            return _customerRepository.Add(customer);
        }

        public bool UpdateCustomer(Customer customer)
        {
            // Tích hợp logic thăng hạng thành viên dựa trên số điểm tích lũy
            UpdateMembershipTier(customer);
            return _customerRepository.Update(customer);
        }

        // Logic tự động thăng hạng khách hàng
        private void UpdateMembershipTier(Customer customer)
        {
            if (customer.LoyaltyPoints >= 1000)
            {
                customer.MembershipTier = "vip_gold";
            }
            else if (customer.LoyaltyPoints >= 500)
            {
                customer.MembershipTier = "vip";
            }
            else
            {
                customer.MembershipTier = "regular";
            }
        }

        public bool DeleteCustomer(int id)
        {
            return _customerRepository.Delete(id);
        }
    }
}
