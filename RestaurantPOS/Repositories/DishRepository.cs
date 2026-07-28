using System.Collections.Generic;
using System.Linq;
using RestaurantPOS.Data;
using RestaurantPOS.Models;

namespace RestaurantPOS.Repositories
{
    public class DishRepository : BaseRepository<Dish>, IDishRepository
    {
        public override List<Dish> GetAll()
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.Dishes.ToList();
            }
        }

        public override Dish? GetById(int id)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.Dishes.Find(id);
            }
        }

        public override bool Add(Dish entity)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                context.Dishes.Add(entity);
                return context.SaveChanges() > 0;
            }
        }

        public override bool Update(Dish entity)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                context.Dishes.Update(entity);
                return context.SaveChanges() > 0;
            }
        }

        public override bool Delete(int id)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                var dish = context.Dishes.Find(id);
                if (dish != null)
                {
                    context.Dishes.Remove(dish);
                    return context.SaveChanges() > 0;
                }
                return false;
            }
        }

        public List<Dish> GetActiveDishes()
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.Dishes
                    .Where(d => d.AvailabilityStatus == "active")
                    .OrderBy(d => d.DishName)
                    .ToList();
            }
        }
    }
}
