using System.Collections.Generic;
using RestaurantPOS.Models;
using RestaurantPOS.Services;
using RestaurantPOS.ViewModels.Billing;
using Xunit;

namespace RestaurantPOS.Tests
{
    public class FakeBillingTableService : ITableService
    {
        public RestaurantTable Table { get; } = new RestaurantTable
        {
            TableId = 1,
            TableName = "Bàn 1",
            Status = "occupied"
        };

        public List<RestaurantTable> GetAllTables() => new List<RestaurantTable> { Table };
        public DiningSession GetActiveSessionByTableId(int tableId) => new DiningSession
        {
            SessionId = 1,
            Status = "open"
        };
        public DiningSession OpenSessionForTable(int tableId, int employeeId, int? customerId) => new DiningSession();
        public bool UpdateTableStatus(int tableId, string status) => true;
        public List<int> GetTableIdsBySessionId(int sessionId) => new List<int> { 1 };
    }

    public class FakeBillingOrderService : IOrderService
    {
        public string ItemStatus { get; set; } = "ready";

        public List<Category> GetAllCategories() => new List<Category>();
        public List<Dish> GetActiveDishes() => new List<Dish>
        {
            new Dish { DishId = 1, DishName = "Món ăn", Price = 100000, AvailabilityStatus = "active" }
        };
        public List<OrderItem> GetOrderItemsBySessionId(int sessionId) => new List<OrderItem>
        {
            new OrderItem { DishId = 1, Quantity = 1, UnitPrice = 100000, Status = ItemStatus }
        };
        public bool PlaceOrder(int sessionId, int employeeId, List<OrderItem> items) => true;
        public bool UpdateOrderItem(OrderItem item) => true;
        public bool DeleteOrderItem(int orderItemId) => true;
    }

    public class FakeBillingCustomerService : ICustomerService
    {
        public List<Customer> GetAllCustomers() => new List<Customer>();
        public List<Customer> SearchCustomers(string keyword) => new List<Customer>();
        public Customer? GetCustomerByPhone(string phone) => null;
        public bool AddCustomer(Customer customer) => true;
        public bool UpdateCustomer(Customer customer) => true;
        public bool DeleteCustomer(int id) => true;
    }

    public class FakeBillingInvoiceService : IInvoiceService
    {
        public Invoice? GetInvoiceBySessionId(int sessionId) => null;
        public bool CreateInvoiceAndCloseSession(
            Invoice invoice,
            List<PaymentDetail> payments,
            List<int> tableIds,
            string nextTableStatus,
            Customer? customer,
            int loyaltyPointsEarned) => true;
    }

    public class BillingViewModelTests
    {
        [Fact]
        public void Checkout_PendingItem_IsBlocked()
        {
            var tableService = new FakeBillingTableService();
            var orderService = new FakeBillingOrderService { ItemStatus = "pending" };
            var viewModel = CreateViewModel(tableService, orderService);
            viewModel.SelectedTable = tableService.Table;
            viewModel.CashAmount = 100000;

            Assert.False(viewModel.CheckoutCommand.CanExecute(null));
        }

        [Fact]
        public void Checkout_ReadyItemAndEnoughCash_IsAllowed()
        {
            var tableService = new FakeBillingTableService();
            var viewModel = CreateViewModel(tableService, new FakeBillingOrderService());
            viewModel.SelectedTable = tableService.Table;
            viewModel.CashAmount = 100000;

            Assert.True(viewModel.CheckoutCommand.CanExecute(null));
        }

        [Fact]
        public void Checkout_CardOverpayment_IsBlocked()
        {
            var tableService = new FakeBillingTableService();
            var viewModel = CreateViewModel(tableService, new FakeBillingOrderService());
            viewModel.SelectedTable = tableService.Table;
            viewModel.CardAmount = 110000;

            Assert.False(viewModel.CheckoutCommand.CanExecute(null));
        }

        private static CheckoutViewModel CreateViewModel(
            ITableService tableService,
            IOrderService orderService)
        {
            return new CheckoutViewModel(
                tableService,
                orderService,
                new FakeBillingCustomerService(),
                new FakeBillingInvoiceService());
        }
    }
}
