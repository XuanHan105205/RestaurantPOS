using System.Collections.Generic;
using RestaurantPOS.Models;

namespace RestaurantPOS.Repositories
{
    public interface IDishRepository : IBaseRepository<Dish>
    {
        List<Dish> GetActiveDishes();
    }
}
