using System.Collections.Generic;
using RestaurantPOS.Models;

namespace RestaurantPOS.Repositories
{
    public class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
    {
        public override List<Customer> GetAll()
        {
            // TODO: Hàn (Leader) will write SQL select query here.
            return new List<Customer>();
        }

        public override Customer GetById(int id)
        {
            // TODO: Hàn (Leader) will write SQL select query here.
            return null;
        }

        public Customer GetByPhone(string phone)
        {
            // TODO: Hàn (Leader) will write SQL select query here.
            return null;
        }

        public override bool Add(Customer entity)
        {
            // TODO: Hàn (Leader) will write SQL insert query here.
            return false;
        }

        public override bool Update(Customer entity)
        {
            // TODO: Hàn (Leader) will write SQL update query here.
            return false;
        }

        public override bool Delete(int id)
        {
            // TODO: Hàn (Leader) will write SQL delete query here.
            return false;
        }
    }
}
