using RestaurantPOS.Models;

namespace RestaurantPOS.Repositories
{
    public interface IEmployeeRepository : IBaseRepository<Employee>
    {
        Employee GetByUsername(string username);
    }
}
