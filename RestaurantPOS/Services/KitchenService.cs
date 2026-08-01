using System;
using System.Collections.Generic;
using RestaurantPOS.Models;
using RestaurantPOS.Repositories;

namespace RestaurantPOS.Services
{
    public class KitchenService : IKitchenService
    {
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly IIngredientService _ingredientService;

        public KitchenService()
            : this(new OrderItemRepository(), new IngredientService())
        {
        }

        public KitchenService(
            IOrderItemRepository orderItemRepository,
            IIngredientService ingredientService)
        {
            _orderItemRepository = orderItemRepository;
            _ingredientService = ingredientService;
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
            string expectedStatus = oldStatus switch
            {
                "pending" => "cooking",
                "cooking" => "ready",
                "ready" => "served",
                _ => string.Empty
            };

            if (newStatus != expectedStatus)
            {
                return false;
            }

            if (newStatus == "ready" && _orderItemRepository is OrderItemRepository concreteRepository)
            {
                bool markedReady = concreteRepository.TryMarkReadyAndDeductStock(orderItemId, AuthService.Instance.CurrentUser?.EmployeeId);
                if (markedReady)
                    AuditTrail.Record("update_status", "order_item", orderItemId, $"Chuyển món từ {oldStatus} sang {newStatus}.");
                return markedReady;
            }

            item.Status = newStatus;
            item.StatusUpdatedAt = DateTime.Now;

            bool success = _orderItemRepository.Update(item);
            if (success && newStatus == "ready")
            {
                if (!_ingredientService.DeductStockForDish(item.DishId, item.Quantity))
                {
                    item.Status = oldStatus;
                    item.StatusUpdatedAt = DateTime.Now;
                    _orderItemRepository.Update(item);
                    return false;
                }
            }
            if (success)
                AuditTrail.Record("update_status", "order_item", orderItemId, $"Chuyển món từ {oldStatus} sang {newStatus}.");
            return success;
        }

        public List<string> GetMissingIngredients(int orderItemId)
        {
            return _orderItemRepository.GetMissingIngredientsForOrderItem(orderItemId);
        }

        public bool CancelOrderItem(int orderItemId, string reason)
        {
            return _orderItemRepository.CancelOrderItem(orderItemId, reason, AuthService.Instance.CurrentUser?.EmployeeId);
        }
    }
}
