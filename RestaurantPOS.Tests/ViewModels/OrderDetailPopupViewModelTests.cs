using System.Collections.Generic;
using System.Linq;
using RestaurantPOS.Models;
using RestaurantPOS.Tests.Fakes;
using RestaurantPOS.ViewModels.Waiter;
using Xunit;

namespace RestaurantPOS.Tests.ViewModels
{
    public class OrderDetailPopupViewModelTests
    {
        private readonly FakeOrderService _orderService;
        private readonly FakeDialogService _dialogService;
        private readonly DiningSession _session;

        public OrderDetailPopupViewModelTests()
        {
            _orderService = new FakeOrderService();
            _dialogService = new FakeDialogService();
            _session = new DiningSession { SessionId = 5, Status = "open" };

            _orderService.Dishes = new List<Dish>
            {
                new Dish { DishId = 1, DishName = "Cơm chiên dương châu", Price = 60000, AvailabilityStatus = "active" }
            };

            _orderService.OrderItems = new List<OrderItem>
            {
                new OrderItem { OrderItemId = 1, OrderId = 10, DishId = 1, Quantity = 2, UnitPrice = 60000, Status = "pending", Note = "Ít cay" }
            };
        }

        [Fact]
        public void LoadItems_CalculatesSessionTotal()
        {
            // Act
            var vm = new OrderDetailPopupViewModel(_session, "Bàn 01", _orderService, _dialogService);

            // Assert
            Assert.Single(vm.OrderedItems);
            Assert.Equal(120000, vm.SessionTotal);
        }

        [Fact]
        public void SaveNote_ShowsSuccessMessage()
        {
            // Arrange
            var vm = new OrderDetailPopupViewModel(_session, "Bàn 01", _orderService, _dialogService);
            var item = vm.OrderedItems.First();
            item.Note = "Không cay";

            // Act
            vm.SaveNoteCommand.Execute(item);

            // Assert
            Assert.Single(_dialogService.LastMessages);
            Assert.Contains("Cập nhật ghi chú thành công!", _dialogService.LastMessages[0]);
            Assert.Equal("Không cay", _orderService.OrderItems.First().Note);
        }

        [Fact]
        public void CancelItem_Confirmed_UpdatesItemStatusToCancelled()
        {
            // Arrange
            _dialogService.ConfirmResult = true;
            var vm = new OrderDetailPopupViewModel(_session, "Bàn 01", _orderService, _dialogService);
            var item = vm.OrderedItems.First();

            // Act
            vm.CancelItemCommand.Execute(item);

            // Assert
            Assert.Single(_dialogService.LastConfirmations);
            Assert.Equal("cancelled", _orderService.OrderItems.First().Status);
        }

        [Fact]
        public void CancelItem_Rejected_DoesNotChangeStatus()
        {
            // Arrange
            _dialogService.ConfirmResult = false;
            var vm = new OrderDetailPopupViewModel(_session, "Bàn 01", _orderService, _dialogService);
            var item = vm.OrderedItems.First();

            // Act
            vm.CancelItemCommand.Execute(item);

            // Assert
            Assert.Single(_dialogService.LastConfirmations);
            Assert.Equal("pending", _orderService.OrderItems.First().Status);
        }

        [Fact]
        public void CloseCommand_TriggersRequestCloseEvent()
        {
            // Arrange
            var vm = new OrderDetailPopupViewModel(_session, "Bàn 01", _orderService, _dialogService);
            bool eventFired = false;
            vm.RequestClose += (result) => eventFired = true;

            // Act
            vm.CloseCommand.Execute(null);

            // Assert
            Assert.True(eventFired);
        }
    }
}
