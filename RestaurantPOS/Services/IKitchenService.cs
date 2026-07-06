using System.Collections.Generic;
using RestaurantPOS.Repositories;

namespace RestaurantPOS.Services
{
    public interface IKitchenService
    {
        List<KitchenOrderItemDto> GetActiveKitchenItems();
        List<KitchenOrderItemDto> GetServedKitchenItemsToday();
        bool UpdateOrderItemStatus(int orderItemId, string newStatus);
    }
}
