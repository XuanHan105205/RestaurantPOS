using System.Collections.Generic;
using System.Linq;
using RestaurantPOS.Models;
using RestaurantPOS.Data;

namespace RestaurantPOS.Repositories
{
    public class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
    {
        public override List<Customer> GetAll()
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.Customers.ToList();
            }
        }

        public override Customer GetById(int id)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.Customers.Find(id);
            }
        }

        public Customer GetByPhone(string phone)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.Customers.FirstOrDefault(c => c.Phone == phone);
            }
        }

        public override bool Add(Customer entity)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                context.Customers.Add(entity);
                return context.SaveChanges() > 0;
            }
        }

        public override bool Update(Customer entity)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                context.Customers.Update(entity);
                return context.SaveChanges() > 0;
            }
        }

        public override bool Delete(int id)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                var customer = context.Customers.Find(id);
                if (customer != null)
                {
                    context.Customers.Remove(customer);
                    return context.SaveChanges() > 0;
                }
                return false;
            }
        }
    }
}
