using System;
using System.Collections.Generic;
using RestaurantPOS.Models;
using RestaurantPOS.Repositories;
using RestaurantPOS.Services;
using Xunit;

namespace RestaurantPOS.Tests
{
    public class FakeKitchenOrderItemRepository : IOrderItemRepository
    {
        public OrderItem? Item { get; set; }

        public List<OrderItem> GetAll() => new List<OrderItem>();
        public OrderItem? GetById(int id) => Item;
        public bool Add(OrderItem entity) => true;
        public bool Update(OrderItem entity)
        {
            Item = entity;
            return true;
        }
        public bool Delete(int id) => true;
        public List<KitchenOrderItemDto> GetActiveKitchenItems() => new List<KitchenOrderItemDto>();
        public List<KitchenOrderItemDto> GetServedKitchenItemsToday() => new List<KitchenOrderItemDto>();
    }

    public class FakeKitchenIngredientService : IIngredientService
    {
        public bool DeductResult { get; set; } = true;
        public int DeductCallCount { get; private set; }

        public List<Ingredient> GetAllIngredients() => new List<Ingredient>();
        public Ingredient? GetIngredientById(int id) => null;
        public bool AddIngredient(Ingredient ingredient) => true;
        public bool UpdateIngredient(Ingredient ingredient) => true;
        public bool DeleteIngredient(int id) => true;
        public List<Ingredient> GetLowStockIngredients() => new List<Ingredient>();
        public bool DeductStockForDish(int dishId, int quantity)
        {
            DeductCallCount++;
            return DeductResult;
        }
    }

    public class KitchenServiceTests
    {
        [Fact]
        public void UpdateStatus_PendingToServed_ReturnsFalse()
        {
            var repository = CreateRepository("pending");
            var service = new KitchenService(repository, new FakeKitchenIngredientService());

            bool result = service.UpdateOrderItemStatus(1, "served");

            Assert.False(result);
            Assert.Equal("pending", repository.Item!.Status);
        }

        [Fact]
        public void UpdateStatus_CookingToReady_DeductsStock()
        {
            var repository = CreateRepository("cooking");
            var ingredientService = new FakeKitchenIngredientService();
            var service = new KitchenService(repository, ingredientService);

            bool result = service.UpdateOrderItemStatus(1, "ready");

            Assert.True(result);
            Assert.Equal("ready", repository.Item!.Status);
            Assert.Equal(1, ingredientService.DeductCallCount);
        }

        [Fact]
        public void UpdateStatus_NotEnoughStock_RestoresCookingStatus()
        {
            var repository = CreateRepository("cooking");
            var ingredientService = new FakeKitchenIngredientService { DeductResult = false };
            var service = new KitchenService(repository, ingredientService);

            bool result = service.UpdateOrderItemStatus(1, "ready");

            Assert.False(result);
            Assert.Equal("cooking", repository.Item!.Status);
        }

        private static FakeKitchenOrderItemRepository CreateRepository(string status)
        {
            return new FakeKitchenOrderItemRepository
            {
                Item = new OrderItem
                {
                    OrderItemId = 1,
                    DishId = 1,
                    Quantity = 1,
                    Status = status,
                    StatusUpdatedAt = DateTime.Now
                }
            };
        }
    }
}
