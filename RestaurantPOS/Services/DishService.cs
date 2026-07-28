using System.Collections.Generic;
using RestaurantPOS.Models;
using RestaurantPOS.Repositories;

namespace RestaurantPOS.Services
{
    public class DishService : IDishService
    {
        private readonly IDishRepository _dishRepository;

        public DishService()
        {
            _dishRepository = new DishRepository();
        }

        public DishService(IDishRepository dishRepository)
        {
            _dishRepository = dishRepository;
        }

        public List<Dish> GetAllDishes()
        {
            return _dishRepository.GetAll() ?? new List<Dish>();
        }

        public List<Dish> GetActiveDishes()
        {
            return _dishRepository.GetActiveDishes() ?? new List<Dish>();
        }

        public Dish? GetDishById(int id)
        {
            if (id <= 0) return null;
            return _dishRepository.GetById(id);
        }
    }
}
