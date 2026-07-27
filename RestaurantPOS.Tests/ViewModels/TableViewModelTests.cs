using System.Collections.Generic;
using System.Linq;
using RestaurantPOS.Models;
using RestaurantPOS.Tests.Fakes;
using RestaurantPOS.ViewModels.Waiter;
using Xunit;

namespace RestaurantPOS.Tests.ViewModels
{
    public class TableViewModelTests
    {
        private readonly FakeTableService _tableService;
        private readonly FakeCustomerService _customerService;
        private readonly FakeDialogService _dialogService;

        public TableViewModelTests()
        {
            _tableService = new FakeTableService();
            _customerService = new FakeCustomerService();
            _dialogService = new FakeDialogService();

            _tableService.Tables = new List<RestaurantTable>
            {
                new RestaurantTable { TableId = 1, TableName = "Bàn 01", Status = "available", Capacity = 4 },
                new RestaurantTable { TableId = 2, TableName = "Bàn 02", Status = "occupied", Capacity = 4 },
                new RestaurantTable { TableId = 3, TableName = "Bàn 03", Status = "needs_cleaning", Capacity = 6 }
            };

            _customerService.Customers = new List<Customer>
            {
                new Customer { CustomerId = 10, FullName = "Nguyễn Văn A", Phone = "0901234567", MembershipTier = "Vàng" }
            };
        }

        [Fact]
        public void LoadTables_PopulatesTablesCollection()
        {
            // Act
            var vm = new TableViewModel(_tableService, _customerService, _dialogService);

            // Assert
            Assert.Equal(3, vm.Tables.Count);
        }

        [Fact]
        public void SelectTable_Available_SetsIsOpenSessionAllowedTrue()
        {
            // Arrange
            var vm = new TableViewModel(_tableService, _customerService, _dialogService);

            // Act
            vm.SelectedTable = vm.Tables.First(t => t.TableId == 1);

            // Assert
            Assert.True(vm.IsOpenSessionAllowed);
            Assert.False(vm.IsCleaningAllowed);
        }

        [Fact]
        public void SearchCustomer_Found_UpdatesSelectedCustomer()
        {
            // Arrange
            var vm = new TableViewModel(_tableService, _customerService, _dialogService);
            vm.CustomerPhoneSearch = "0901234567";

            // Act
            vm.SearchCustomerCommand.Execute(null);

            // Assert
            Assert.NotNull(vm.SelectedCustomer);
            Assert.Equal("Nguyễn Văn A", vm.SelectedCustomer.FullName);
            Assert.Contains("Tìm thấy: Nguyễn Văn A", vm.CustomerSearchResultMessage);
        }

        [Fact]
        public void OpenSession_Success_ChangesTableStatusToOccupied()
        {
            // Arrange
            var vm = new TableViewModel(_tableService, _customerService, _dialogService);
            vm.SelectedTable = vm.Tables.First(t => t.TableId == 1);

            // Act
            vm.OpenSessionCommand.Execute(null);

            // Assert
            Assert.Equal("occupied", vm.SelectedTable?.Status);
            Assert.NotNull(vm.ActiveSession);
        }

        [Fact]
        public void OpenSession_ThrowsException_ShowsErrorDialog()
        {
            // Arrange
            _tableService.ShouldThrowOnOpenSession = true;
            var vm = new TableViewModel(_tableService, _customerService, _dialogService);
            vm.SelectedTable = vm.Tables.First(t => t.TableId == 1);

            // Act
            vm.OpenSessionCommand.Execute(null);

            // Assert
            Assert.Single(_dialogService.LastMessages);
            Assert.Contains("Lỗi khi mở bàn", _dialogService.LastMessages[0]);
        }

        [Fact]
        public void ViewOrderDetails_CallsShowOrderDetailPopup()
        {
            // Arrange
            var vm = new TableViewModel(_tableService, _customerService, _dialogService);
            vm.SelectedTable = vm.Tables.First(t => t.TableId == 2);
            vm.ActiveSession = new DiningSession { SessionId = 100, Status = "open" };

            // Act
            vm.ViewOrderDetailsCommand.Execute(null);

            // Assert
            Assert.True(_dialogService.ShowOrderDetailPopupCalled);
        }
    }
}
