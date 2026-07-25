using System.Collections.Generic;
using RestaurantPOS.Models;

namespace RestaurantPOS.Services
{
    public interface IDishService
    {
        List<Dish> GetAllDishes();
        List<Dish> GetActiveDishes();
        Dish GetDishById(int id);
    }
}
