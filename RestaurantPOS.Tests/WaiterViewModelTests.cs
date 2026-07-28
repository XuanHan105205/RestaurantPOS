using System.Collections.Generic;
using RestaurantPOS.Models;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Waiter;
using Xunit;

namespace RestaurantPOS.Tests
{
    public class FakeWaiterTableService : ITableService
    {
        public List<RestaurantTable> Tables { get; } = new();
        public DiningSession? ActiveSession { get; set; }
        public string LastUpdatedStatus { get; private set; } = string.Empty;

        public List<RestaurantTable> GetAllTables() => new(Tables);
        public DiningSession? GetActiveSessionByTableId(int tableId) => ActiveSession;
        public DiningSession OpenSessionForTable(int tableId, int employeeId, int? customerId)
            => ActiveSession ?? new DiningSession { SessionId = 1 };

        public bool UpdateTableStatus(int tableId, string status)
        {
            LastUpdatedStatus = status;
            return true;
        }

        public List<int> GetTableIdsBySessionId(int sessionId) => new();
    }

    public class FakeWaiterCustomerService : ICustomerService
    {
        public List<Customer> Customers { get; } = new();

        public List<Customer> GetAllCustomers() => new(Customers);
        public List<Customer> SearchCustomers(string keyword) => new(Customers);
        public Customer? GetCustomerByPhone(string phone) => Customers.Find(c => c.Phone == phone);
        public bool AddCustomer(Customer customer) => true;
        public bool UpdateCustomer(Customer customer) => true;
        public bool DeleteCustomer(int id) => true;
    }

    public class FakeWaiterDialogService : IDialogService
    {
        public bool ConfirmResult { get; set; } = true;
        public bool Confirm(string title, string message) => ConfirmResult;
        public void ShowMessage(string title, string message) { }
    }

    public class FakeOrderDetailDialogService : IOrderDetailDialogService
    {
        public void Show(DiningSession session, string tableName) { }
    }

    public class FakeWaiterOrderService : IOrderService
    {
        public List<Category> Categories { get; } = new();
        public List<Dish> Dishes { get; } = new();
        public List<OrderItem> OrderItems { get; } = new();
        public int UpdateCallCount { get; private set; }

        public List<Category> GetAllCategories() => new(Categories);
        public List<Dish> GetActiveDishes() => new(Dishes);
        public List<OrderItem> GetOrderItemsBySessionId(int sessionId) => new(OrderItems);
        public bool PlaceOrder(int sessionId, int employeeId, List<OrderItem> items) => true;

        public bool UpdateOrderItem(OrderItem item)
        {
            UpdateCallCount++;
            return true;
        }

        public bool DeleteOrderItem(int orderItemId) => true;
    }

    public class WaiterViewModelTests
    {
        [Fact]
        public void SelectAvailableTable_AllowsOpeningSession()
        {
            var tableService = new FakeWaiterTableService();
            var table = new RestaurantTable { TableId = 1, Status = "available" };
            tableService.Tables.Add(table);
            var viewModel = CreateTableViewModel(tableService);

            viewModel.SelectedTable = table;

            Assert.True(viewModel.IsOpenSessionAllowed);
            Assert.False(viewModel.IsCleaningAllowed);
        }

        [Fact]
        public void CleanTable_ChangesStatusToAvailable()
        {
            var tableService = new FakeWaiterTableService();
            var table = new RestaurantTable { TableId = 1, Status = "needs_cleaning" };
            tableService.Tables.Add(table);
            var viewModel = CreateTableViewModel(tableService);
            viewModel.SelectedTable = table;

            viewModel.CleanTableCommand.Execute(null);

            Assert.Equal("available", tableService.LastUpdatedStatus);
        }

        [Fact]
        public void SearchCustomer_ByPhone_SelectsCustomer()
        {
            var customerService = new FakeWaiterCustomerService();
            customerService.Customers.Add(new Customer
            {
                CustomerId = 2,
                FullName = "Nguyễn Thị Hoa",
                Phone = "0909090909"
            });
            var viewModel = CreateTableViewModel(new FakeWaiterTableService(), customerService);
            viewModel.CustomerPhoneSearch = "0909090909";

            viewModel.SearchCustomerCommand.Execute(null);

            Assert.NotNull(viewModel.SelectedCustomer);
            Assert.Equal("Nguyễn Thị Hoa", viewModel.SelectedCustomer.FullName);
        }

        [Fact]
        public void SearchMenu_ByName_FiltersDishes()
        {
            var orderService = CreateOrderService();
            var viewModel = CreateOrderViewModel(orderService);

            viewModel.SearchText = "cơm";

            Assert.Single(viewModel.FilteredDishes);
            Assert.Equal("Cơm gà", viewModel.FilteredDishes[0].DishName);
        }

        [Fact]
        public void AddSameDishTwice_IncreasesQuantity()
        {
            var orderService = CreateOrderService();
            var viewModel = CreateOrderViewModel(orderService);
            var dish = orderService.Dishes[0];

            viewModel.AddToCartCommand.Execute(dish);
            viewModel.AddToCartCommand.Execute(dish);

            Assert.Single(viewModel.Cart);
            Assert.Equal(2, viewModel.Cart[0].Quantity);
        }

        [Fact]
        public void DecreaseLastQuantity_RemovesDishFromCart()
        {
            var orderService = CreateOrderService();
            var viewModel = CreateOrderViewModel(orderService);
            viewModel.AddToCartCommand.Execute(orderService.Dishes[0]);

            viewModel.DecrementCartQuantityCommand.Execute(viewModel.Cart[0]);

            Assert.Empty(viewModel.Cart);
        }

        [Fact]
        public void PendingOrderItem_CanIncreaseQuantity()
        {
            var orderService = CreateOrderService();
            orderService.OrderItems.Add(new OrderItem
            {
                OrderItemId = 1,
                DishId = 1,
                Quantity = 1,
                UnitPrice = 50000,
                Status = "pending"
            });
            var viewModel = CreateOrderDetailViewModel(orderService);

            viewModel.IncrementCommand.Execute(viewModel.OrderedItems[0]);

            Assert.Equal(2, viewModel.OrderedItems[0].Quantity);
            Assert.Equal(1, orderService.UpdateCallCount);
        }

        [Fact]
        public void CookingOrderItem_CannotChangeQuantity()
        {
            var orderService = CreateOrderService();
            orderService.OrderItems.Add(new OrderItem
            {
                OrderItemId = 1,
                DishId = 1,
                Quantity = 1,
                UnitPrice = 50000,
                Status = "cooking"
            });
            var viewModel = CreateOrderDetailViewModel(orderService);

            viewModel.IncrementCommand.Execute(viewModel.OrderedItems[0]);

            Assert.Equal(1, viewModel.OrderedItems[0].Quantity);
            Assert.Equal(0, orderService.UpdateCallCount);
        }

        private static TableViewModel CreateTableViewModel(
            FakeWaiterTableService tableService,
            FakeWaiterCustomerService? customerService = null)
        {
            return new TableViewModel(
                tableService,
                customerService ?? new FakeWaiterCustomerService(),
                new FakeWaiterDialogService(),
                new FakeOrderDetailDialogService());
        }

        private static FakeWaiterOrderService CreateOrderService()
        {
            var service = new FakeWaiterOrderService();
            service.Categories.Add(new Category { CategoryId = 1, CategoryName = "Món chính" });
            service.Dishes.Add(new Dish { DishId = 1, DishName = "Cơm gà", CategoryId = 1, Price = 50000 });
            service.Dishes.Add(new Dish { DishId = 2, DishName = "Phở bò", CategoryId = 1, Price = 60000 });
            return service;
        }

        private static OrderViewModel CreateOrderViewModel(FakeWaiterOrderService service)
        {
            return new OrderViewModel(
                new RestaurantTable { TableId = 1, TableName = "Bàn 1" },
                new DiningSession { SessionId = 1 },
                service,
                new FakeWaiterDialogService());
        }

        private static OrderDetailPopupViewModel CreateOrderDetailViewModel(FakeWaiterOrderService service)
        {
            return new OrderDetailPopupViewModel(
                new DiningSession { SessionId = 1 },
                "Bàn 1",
                service,
                new FakeWaiterDialogService());
        }
    }
}
