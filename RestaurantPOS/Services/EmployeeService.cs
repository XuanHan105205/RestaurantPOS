using System.Collections.Generic;
using RestaurantPOS.Models;
using RestaurantPOS.Repositories;

namespace RestaurantPOS.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService()
        {
            _employeeRepository = new EmployeeRepository();
        }

        public EmployeeService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public List<Employee> GetAllEmployees()
        {
            return _employeeRepository.GetAll() ?? new List<Employee>();
        }

        public Employee? GetEmployeeById(int id)
        {
            if (id <= 0) return null;
            return _employeeRepository.GetById(id);
        }
    }
}
