using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public List<Customer> GetAllCustomers()
        {
            return _customerRepository.GetAll() ?? new List<Customer>();
        }

        public Customer? GetCustomerByPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return null;
            return _customerRepository.GetByPhone(phone.Trim());
        }

        public bool AddCustomer(Customer customer)
        {
            if (!ValidateCustomer(customer, out _))
            {
                return false;
            }

            // Kiểm tra trùng SĐT
            var existing = GetCustomerByPhone(customer.Phone);
            if (existing != null)
            {
                return false;
            }

            customer.FullName = customer.FullName.Trim();
            customer.Phone = customer.Phone.Trim();
            if (string.IsNullOrEmpty(customer.MembershipTier))
            {
                customer.MembershipTier = "regular";
            }
            UpdateMembershipTier(customer);

            return _customerRepository.Add(customer);
        }

        public bool UpdateCustomer(Customer customer)
        {
            if (!ValidateCustomer(customer, out _))
            {
                return false;
            }

            // Kiểm tra trùng SĐT với khách hàng khác
            var existing = GetAllCustomers().FirstOrDefault(c => 
                c.CustomerId != customer.CustomerId && 
                c.Phone != null && 
                c.Phone.Equals(customer.Phone.Trim(), StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                return false;
            }

            customer.FullName = customer.FullName.Trim();
            customer.Phone = customer.Phone.Trim();
            UpdateMembershipTier(customer);

            return _customerRepository.Update(customer);
        }

        public bool DeleteCustomer(int id)
        {
            if (id <= 0) return false;
            return _customerRepository.Delete(id);
        }

        public double GetDiscountRateByCustomer(Customer? customer)
        {
            if (customer == null || string.IsNullOrEmpty(customer.MembershipTier))
            {
                return 0.0;
            }

            return customer.MembershipTier.ToLower() switch
            {
                "vip_gold" => 0.10, // Giảm 10%
                "vip" => 0.05,      // Giảm 5%
                _ => 0.0            // Regular 0%
            };
        }

        public bool AddLoyaltyPoints(int customerId, int pointsEarned)
        {
            if (customerId <= 0 || pointsEarned <= 0) return false;

            var customer = _customerRepository.GetById(customerId);
            if (customer == null) return false;

            customer.LoyaltyPoints += pointsEarned;
            UpdateMembershipTier(customer);
            return _customerRepository.Update(customer);
        }

        public bool ValidateCustomer(Customer customer, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (customer == null)
            {
                errorMessage = "Thông tin khách hàng không hợp lệ.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(customer.FullName))
            {
                errorMessage = "Họ tên khách hàng không được để trống.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(customer.Phone))
            {
                errorMessage = "Số điện thoại không được để trống.";
                return false;
            }

            string cleanPhone = customer.Phone.Trim();
            if (!Regex.IsMatch(cleanPhone, @"^[0-9]{10,11}$"))
            {
                errorMessage = "Số điện thoại phải từ 10 - 11 chữ số hợp lệ.";
                return false;
            }

            if (customer.LoyaltyPoints < 0)
            {
                errorMessage = "Điểm tích lũy không được âm.";
                return false;
            }

            return true;
        }

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
    }
}
