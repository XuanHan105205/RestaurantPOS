using System;
using System.Collections.Generic;
using RestaurantPOS.Models;
using RestaurantPOS.Repositories;

namespace RestaurantPOS.Services
{
    public class KitchenService : IKitchenService
    {
        private readonly IOrderItemRepository _orderItemRepository;

        public KitchenService()
        {
            _orderItemRepository = new OrderItemRepository();
        }

        public List<KitchenOrderItemDto> GetActiveKitchenItems()
        {
            return _orderItemRepository.GetActiveKitchenItems();
        }

        public List<KitchenOrderItemDto> GetServedKitchenItemsToday()
        {
            return _orderItemRepository.GetServedKitchenItemsToday();
        }

        public bool UpdateOrderItemStatus(int orderItemId, string newStatus)
        {
            var item = _orderItemRepository.GetById(orderItemId);
            if (item == null) return false;

            string oldStatus = item.Status;
            item.Status = newStatus;
            item.StatusUpdatedAt = DateTime.Now;

            bool success = _orderItemRepository.Update(item);
            if (success)
            {
                // Trừ kho tự động khi trạng thái chuyển sang "ready" (nếu trước đó chưa trừ)
                if (newStatus == "ready" && oldStatus != "ready" && oldStatus != "served")
                {
                    using (var context = new Data.RestaurantPOSDbContext())
                    {
                        var ingredientRepo = new IngredientRepository();
                        ingredientRepo.DeductStockForDish(item.DishId, item.Quantity, context);
                        context.SaveChanges();
                    }
                }
            }
            return success;
        }
    }
}
