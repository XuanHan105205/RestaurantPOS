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

            item.Status = newStatus;
            item.StatusUpdatedAt = DateTime.Now;

            return _orderItemRepository.Update(item);
        }
    }
}
