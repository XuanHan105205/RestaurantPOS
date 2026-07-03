using System.Collections.Generic;
using RestaurantPOS.Models;

namespace RestaurantPOS.Repositories
{
    public class EmployeeRepository : BaseRepository<Employee>, IEmployeeRepository
    {
        public override List<Employee> GetAll()
        {
            // TODO: Hàn will write SQL select query here.
            return new List<Employee>();
        }

        public override Employee GetById(int id)
        {
            // TODO: Hàn will write SQL select query here.
            return null;
        }

        public Employee GetByUsername(string username)
        {
            // TODO: Hàn will write SQL select query here.
            return null;
        }

        public override bool Add(Employee entity)
        {
            // TODO: Hàn will write SQL insert query here.
            return false;
        }

        public override bool Update(Employee entity)
        {
            // TODO: Hàn will write SQL update query here.
            return false;
        }

        public override bool Delete(int id)
        {
            // TODO: Hàn will write SQL delete query here.
            return false;
        }
    }
}
