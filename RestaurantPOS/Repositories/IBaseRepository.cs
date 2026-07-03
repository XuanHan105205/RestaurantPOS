using System.Collections.Generic;

namespace RestaurantPOS.Repositories
{
    public interface IBaseRepository<T> where T : class
    {
        List<T> GetAll();
        T GetById(int id);
        bool Add(T entity);
        bool Update(T entity);
        bool Delete(int id);
    }
}
