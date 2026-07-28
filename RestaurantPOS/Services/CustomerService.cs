using System.Collections.Generic;
using RestaurantPOS.Models;
using RestaurantPOS.Repositories;

namespace RestaurantPOS.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService()
            : this(new CustomerRepository())
        {
        }

        // Constructor này giúp kiểm thử mà không cần kết nối database thật.
        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public List<Customer> GetAllCustomers()
        {
            return _customerRepository.GetAll();
        }

        public List<Customer> SearchCustomers(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return GetAllCustomers();
            }

            string searchValue = keyword.Trim().ToLower();
            var results = new List<Customer>();

            foreach (var customer in GetAllCustomers())
            {
                bool nameMatches = customer.FullName.ToLower().Contains(searchValue);
                bool phoneMatches = customer.Phone.Contains(searchValue);

                if (nameMatches || phoneMatches)
                {
                    results.Add(customer);
                }
            }

            return results;
        }

        public Customer? GetCustomerByPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return null;
            }

            return _customerRepository.GetByPhone(phone.Trim());
        }

        public bool AddCustomer(Customer customer)
        {
            if (!IsValidCustomer(customer))
            {
                return false;
            }

            customer.FullName = customer.FullName.Trim();
            customer.Phone = customer.Phone.Trim();

            if (_customerRepository.GetByPhone(customer.Phone) != null)
            {
                return false;
            }

            UpdateMembershipTier(customer);
            return _customerRepository.Add(customer);
        }

        public bool UpdateCustomer(Customer customer)
        {
            if (!IsValidCustomer(customer))
            {
                return false;
            }

            customer.FullName = customer.FullName.Trim();
            customer.Phone = customer.Phone.Trim();

            var customerWithSamePhone = _customerRepository.GetByPhone(customer.Phone);
            if (customerWithSamePhone != null &&
                customerWithSamePhone.CustomerId != customer.CustomerId)
            {
                return false;
            }

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
            if (id <= 0)
            {
                return false;
            }

            return _customerRepository.Delete(id);
        }

        private bool IsValidCustomer(Customer customer)
        {
            if (customer == null ||
                string.IsNullOrWhiteSpace(customer.FullName) ||
                string.IsNullOrWhiteSpace(customer.Phone) ||
                customer.LoyaltyPoints < 0)
            {
                return false;
            }

            string phone = customer.Phone.Trim();
            if (phone.Length < 9 || phone.Length > 15)
            {
                return false;
            }

            foreach (char character in phone)
            {
                if (!char.IsDigit(character))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
