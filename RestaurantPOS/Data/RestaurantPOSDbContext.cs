using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RestaurantPOS.Models;

namespace RestaurantPOS.Data
{
    public class RestaurantPOSDbContext : DbContext
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<RestaurantTable> RestaurantTables { get; set; }
        public DbSet<DiningSession> DiningSessions { get; set; }
        public DbSet<TableSession> TableSessions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<StockReceipt> StockReceipts { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<PaymentDetail> PaymentDetails { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                string connectionString = config.GetConnectionString("RestaurantPOSConnection") 
                    ?? "Server=localhost;Database=RestaurantPOS;Trusted_Connection=True;TrustServerCertificate=True;";

                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 1. Map Employee
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("employees");
                entity.HasKey(e => e.EmployeeId);
                entity.Property(e => e.EmployeeId).HasColumnName("employee_id");
                entity.Property(e => e.FullName).HasColumnName("full_name").IsRequired();
                entity.Property(e => e.Username).HasColumnName("username").IsRequired();
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired();
                entity.Property(e => e.Role).HasColumnName("role").IsRequired();
                entity.Property(e => e.Phone).HasColumnName("phone");
                entity.Property(e => e.IsActive).HasColumnName("is_active").IsRequired();
            });

            // 2. Map Customer
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("customers");
                entity.HasKey(c => c.CustomerId);
                entity.Property(c => c.CustomerId).HasColumnName("customer_id");
                entity.Property(c => c.FullName).HasColumnName("full_name").IsRequired();
                entity.Property(c => c.Phone).HasColumnName("phone");
                entity.Property(c => c.MembershipTier).HasColumnName("membership_tier").IsRequired();
                entity.Property(c => c.LoyaltyPoints).HasColumnName("loyalty_points").IsRequired();
            });

            // 3. Map RestaurantTable
            modelBuilder.Entity<RestaurantTable>(entity =>
            {
                entity.ToTable("restaurant_tables");
                entity.HasKey(t => t.TableId);
                entity.Property(t => t.TableId).HasColumnName("table_id");
                entity.Property(t => t.TableName).HasColumnName("table_name").IsRequired();
                entity.Property(t => t.Capacity).HasColumnName("capacity");
                entity.Property(t => t.Status).HasColumnName("status").IsRequired();
                entity.Property(t => t.Area).HasColumnName("area");
            });

            // 4. Map DiningSession
            modelBuilder.Entity<DiningSession>(entity =>
            {
                entity.ToTable("dining_sessions");
                entity.HasKey(s => s.SessionId);
                entity.Property(s => s.SessionId).HasColumnName("session_id");
                entity.Property(s => s.OpenedAt).HasColumnName("opened_at").IsRequired();
                entity.Property(s => s.ClosedAt).HasColumnName("closed_at");
                entity.Property(s => s.OpenedByEmployeeId).HasColumnName("opened_by_employee_id").IsRequired();
                entity.Property(s => s.CustomerId).HasColumnName("customer_id");
                entity.Property(s => s.Status).HasColumnName("status").IsRequired();
            });

            // 5. Map TableSession (Composite Primary Key)
            modelBuilder.Entity<TableSession>(entity =>
            {
                entity.ToTable("table_sessions");
                entity.HasKey(ts => new { ts.TableId, ts.SessionId });
                entity.Property(ts => ts.TableId).HasColumnName("table_id");
                entity.Property(ts => ts.SessionId).HasColumnName("session_id");
            });

            // 6. Map Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("categories");
                entity.HasKey(c => c.CategoryId);
                entity.Property(c => c.CategoryId).HasColumnName("category_id");
                entity.Property(c => c.CategoryName).HasColumnName("category_name").IsRequired();
            });

            // 7. Map Dish
            modelBuilder.Entity<Dish>(entity =>
            {
                entity.ToTable("dishes");
                entity.HasKey(d => d.DishId);
                entity.Property(d => d.DishId).HasColumnName("dish_id");
                entity.Property(d => d.DishName).HasColumnName("dish_name").IsRequired();
                entity.Property(d => d.Price).HasColumnName("price").IsRequired();
                entity.Property(d => d.CategoryId).HasColumnName("category_id");
                entity.Property(d => d.AvailabilityStatus).HasColumnName("availability_status").IsRequired();
                entity.Property(d => d.ImageUrl).HasColumnName("image_url");
            });

            // 8. Map Order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("orders");
                entity.HasKey(o => o.OrderId);
                entity.Property(o => o.OrderId).HasColumnName("order_id");
                entity.Property(o => o.SessionId).HasColumnName("session_id").IsRequired();
                entity.Property(o => o.CreatedByEmployeeId).HasColumnName("created_by_employee_id").IsRequired();
                entity.Property(o => o.OrderedAt).HasColumnName("ordered_at").IsRequired();
            });

            // 9. Map OrderItem
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("order_items");
                entity.HasKey(oi => oi.OrderItemId);
                entity.Property(oi => oi.OrderItemId).HasColumnName("order_item_id");
                entity.Property(oi => oi.OrderId).HasColumnName("order_id").IsRequired();
                entity.Property(oi => oi.DishId).HasColumnName("dish_id").IsRequired();
                entity.Property(oi => oi.Quantity).HasColumnName("quantity").IsRequired();
                entity.Property(oi => oi.UnitPrice).HasColumnName("unit_price").IsRequired();
                entity.Property(oi => oi.Status).HasColumnName("status").IsRequired();
                entity.Property(oi => oi.Note).HasColumnName("note");
                entity.Property(oi => oi.StatusUpdatedAt).HasColumnName("status_updated_at");
            });

            // 10. Map Ingredient
            modelBuilder.Entity<Ingredient>(entity =>
            {
                entity.ToTable("ingredients");
                entity.HasKey(i => i.IngredientId);
                entity.Property(i => i.IngredientId).HasColumnName("ingredient_id");
                entity.Property(i => i.IngredientName).HasColumnName("ingredient_name").IsRequired();
                entity.Property(i => i.Unit).HasColumnName("unit").IsRequired();
                entity.Property(i => i.StockQuantity).HasColumnName("stock_quantity").IsRequired();
                entity.Property(i => i.MinStockAlert).HasColumnName("min_stock_alert");
            });

            // 11. Map Recipe (Composite Primary Key)
            modelBuilder.Entity<Recipe>(entity =>
            {
                entity.ToTable("recipes");
                entity.HasKey(r => new { r.DishId, r.IngredientId });
                entity.Property(r => r.DishId).HasColumnName("dish_id");
                entity.Property(r => r.IngredientId).HasColumnName("ingredient_id");
                entity.Property(r => r.QuantityPerServing).HasColumnName("quantity_per_serving").IsRequired();
            });

            // 12. Map StockReceipt
            modelBuilder.Entity<StockReceipt>(entity =>
            {
                entity.ToTable("stock_receipts");
                entity.HasKey(sr => sr.ReceiptId);
                entity.Property(sr => sr.ReceiptId).HasColumnName("receipt_id");
                entity.Property(sr => sr.IngredientId).HasColumnName("ingredient_id").IsRequired();
                entity.Property(sr => sr.Quantity).HasColumnName("quantity").IsRequired();
                entity.Property(sr => sr.UnitCost).HasColumnName("unit_cost");
                entity.Property(sr => sr.ReceivedAt).HasColumnName("received_at").IsRequired();
                entity.Property(sr => sr.ReceivedByEmployeeId).HasColumnName("received_by_employee_id");
                entity.Property(sr => sr.Supplier).HasColumnName("supplier");
            });

            // 13. Map Invoice
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.ToTable("invoices");
                entity.HasKey(inv => inv.InvoiceId);
                entity.Property(inv => inv.InvoiceId).HasColumnName("invoice_id");
                entity.Property(inv => inv.SessionId).HasColumnName("session_id").IsRequired();
                entity.Property(inv => inv.Subtotal).HasColumnName("subtotal").IsRequired();
                entity.Property(inv => inv.Discount).HasColumnName("discount").IsRequired();
                entity.Property(inv => inv.TotalAmount).HasColumnName("total_amount").IsRequired();
                entity.Property(inv => inv.PaidAt).HasColumnName("paid_at").IsRequired();
                entity.Property(inv => inv.CashierEmployeeId).HasColumnName("cashier_employee_id");
            });

            // 14. Map PaymentDetail
            modelBuilder.Entity<PaymentDetail>(entity =>
            {
                entity.ToTable("payment_details");
                entity.HasKey(pd => pd.PaymentId);
                entity.Property(pd => pd.PaymentId).HasColumnName("payment_id");
                entity.Property(pd => pd.InvoiceId).HasColumnName("invoice_id").IsRequired();
                entity.Property(pd => pd.Method).HasColumnName("method").IsRequired();
                entity.Property(pd => pd.Amount).HasColumnName("amount").IsRequired();
            });
        }
    }
}
