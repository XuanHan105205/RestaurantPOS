using System.Collections.Generic;
using RestaurantPOS.Models;

namespace RestaurantPOS.Services
{
    public interface IAIService
    {
        List<Dish> GetRecommendedDishes(List<int> currentDishIds);
    }
}
