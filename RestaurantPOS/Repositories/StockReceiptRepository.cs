using System;
using System.Collections.Generic;
using System.Linq;
using RestaurantPOS.Models;
using RestaurantPOS.Data;

namespace RestaurantPOS.Repositories
{
    public class StockReceiptRepository : BaseRepository<StockReceipt>, IStockReceiptRepository
    {
        public override List<StockReceipt> GetAll()
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.StockReceipts.OrderByDescending(sr => sr.ReceivedAt).ToList();
            }
        }

        public override StockReceipt GetById(int id)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.StockReceipts.Find(id);
            }
        }

        public override bool Add(StockReceipt entity)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        // 1. Add the receipt record
                        entity.ReceivedAt = DateTime.Now;
                        context.StockReceipts.Add(entity);
                        context.SaveChanges();

                        // 2. Increase the ingredient stock quantity
                        var ingredient = context.Ingredients.Find(entity.IngredientId);
                        if (ingredient != null)
                        {
                            ingredient.StockQuantity += entity.Quantity;
                            context.Ingredients.Update(ingredient);
                            context.SaveChanges();
                        }
                        else
                        {
                            throw new Exception($"Ingredient with ID {entity.IngredientId} not found.");
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public override bool Update(StockReceipt entity)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                context.StockReceipts.Update(entity);
                return context.SaveChanges() > 0;
            }
        }

        public override bool Delete(int id)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var receipt = context.StockReceipts.Find(id);
                        if (receipt != null)
                        {
                            // 1. Decrease the ingredient stock quantity
                            var ingredient = context.Ingredients.Find(receipt.IngredientId);
                            if (ingredient != null)
                            {
                                ingredient.StockQuantity -= receipt.Quantity;
                                if (ingredient.StockQuantity < 0) ingredient.StockQuantity = 0;
                                context.Ingredients.Update(ingredient);
                                context.SaveChanges();
                            }

                            // 2. Remove the receipt record
                            context.StockReceipts.Remove(receipt);
                            context.SaveChanges();

                            transaction.Commit();
                            return true;
                        }
                        return false;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }

        public List<StockReceipt> GetReceiptsByIngredientId(int ingredientId)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.StockReceipts
                    .Where(sr => sr.IngredientId == ingredientId)
                    .OrderByDescending(sr => sr.ReceivedAt)
                    .ToList();
            }
        }
    }
}
