using RestaurantPOS.Models;

namespace RestaurantPOS.Repositories
{
    public interface ICustomerRepository : IBaseRepository<Customer>
    {
        Customer GetByPhone(string phone);
    }
}
