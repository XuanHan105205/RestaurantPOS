using System.Collections.Generic;
using System.Linq;
using RestaurantPOS.Models;
using RestaurantPOS.Tests.Fakes;
using RestaurantPOS.ViewModels.Waiter;
using Xunit;

namespace RestaurantPOS.Tests.ViewModels
{
    public class OrderViewModelTests
    {
        private readonly FakeOrderService _orderService;
        private readonly FakeDialogService _dialogService;
        private readonly RestaurantTable _table;
        private readonly DiningSession _session;

        public OrderViewModelTests()
        {
            _orderService = new FakeOrderService();
            _dialogService = new FakeDialogService();

            _table = new RestaurantTable { TableId = 1, TableName = "Bàn 01", Status = "occupied" };
            _session = new DiningSession { SessionId = 10, Status = "open" };

            _orderService.Categories = new List<Category>
            {
                new Category { CategoryId = 1, CategoryName = "Khai vị" },
                new Category { CategoryId = 2, CategoryName = "Món chính" }
            };

            _orderService.Dishes = new List<Dish>
            {
                new Dish { DishId = 101, DishName = "Gỏi ngó sen", Price = 80000, CategoryId = 1, AvailabilityStatus = "active" },
                new Dish { DishId = 102, DishName = "Lẩu thái", Price = 250000, CategoryId = 2, AvailabilityStatus = "active" }
            };
        }

        [Fact]
        public void AddToCart_AddsItemAndCalculatesCartTotal()
        {
            // Arrange
            var vm = new OrderViewModel(_table, _session, _orderService, _dialogService);
            var dish = _orderService.Dishes.First();

            // Act
            vm.AddToCartCommand.Execute(dish);

            // Assert
            Assert.Single(vm.Cart);
            Assert.Equal(80000, vm.CartTotal);
        }

        [Fact]
        public void ConfirmOrder_EmptyCart_ShowsWarningDialog()
        {
            // Arrange
            var vm = new OrderViewModel(_table, _session, _orderService, _dialogService);

            // Act
            vm.ConfirmOrderCommand.Execute(null);

            // Assert
            Assert.Single(_dialogService.LastMessages);
            Assert.Contains("Vui lòng chọn món ăn trước khi xác nhận!", _dialogService.LastMessages[0]);
        }

        [Fact]
        public void ConfirmOrder_Success_ShowsSuccessDialogAndPlacesOrder()
        {
            // Arrange
            var vm = new OrderViewModel(_table, _session, _orderService, _dialogService);
            vm.AddToCartCommand.Execute(_orderService.Dishes.First());

            // Act
            vm.ConfirmOrderCommand.Execute(null);

            // Assert
            Assert.Single(_dialogService.LastMessages);
            Assert.Contains("Gọi món thành công!", _dialogService.LastMessages[0]);
            Assert.Single(_orderService.OrderItems);
        }

        [Fact]
        public void ConfirmOrder_Failure_ShowsErrorDialog()
        {
            // Arrange
            _orderService.PlaceOrderResult = false;
            var vm = new OrderViewModel(_table, _session, _orderService, _dialogService);
            vm.AddToCartCommand.Execute(_orderService.Dishes.First());

            // Act
            vm.ConfirmOrderCommand.Execute(null);

            // Assert
            Assert.Single(_dialogService.LastMessages);
            Assert.Contains("Có lỗi xảy ra khi gọi món", _dialogService.LastMessages[0]);
        }
    }
}
