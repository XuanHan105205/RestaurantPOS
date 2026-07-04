using System.Collections.Generic;
using System.Linq;
using RestaurantPOS.Models;
using RestaurantPOS.Data;

namespace RestaurantPOS.Repositories
{
    public class EmployeeRepository : BaseRepository<Employee>, IEmployeeRepository
    {
        public override List<Employee> GetAll()
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.Employees.ToList();
            }
        }

        public override Employee GetById(int id)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.Employees.Find(id);
            }
        }

        public Employee GetByUsername(string username)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.Employees.FirstOrDefault(e => e.Username == username);
            }
        }

        public override bool Add(Employee entity)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                context.Employees.Add(entity);
                return context.SaveChanges() > 0;
            }
        }

        public override bool Update(Employee entity)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                context.Employees.Update(entity);
                return context.SaveChanges() > 0;
            }
        }

        public override bool Delete(int id)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                var employee = context.Employees.Find(id);
                if (employee != null)
                {
                    context.Employees.Remove(employee);
                    return context.SaveChanges() > 0;
                }
                return false;
            }
        }
    }
}
