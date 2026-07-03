using System.Collections.Generic;

namespace RestaurantPOS.Repositories
{
    public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        public abstract List<T> GetAll();
        public abstract T GetById(int id);
        public abstract bool Add(T entity);
        public abstract bool Update(T entity);
        public abstract bool Delete(int id);
    }
}
