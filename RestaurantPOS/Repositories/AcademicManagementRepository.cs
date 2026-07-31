using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Data;
using RestaurantPOS.Models;

namespace RestaurantPOS.Repositories
{
    public class AcademicManagementRepository
    {
        public List<Employee> GetEmployees() { using var db = new RestaurantPOSDbContext(); return db.Employees.OrderBy(e => e.FullName).ToList(); }
        public List<Category> GetCategories() { using var db = new RestaurantPOSDbContext(); return db.Categories.OrderBy(c => c.CategoryName).ToList(); }
        public List<Dish> GetDishes() { using var db = new RestaurantPOSDbContext(); return db.Dishes.OrderBy(d => d.DishName).ToList(); }
        public List<RestaurantTable> GetTables() { using var db = new RestaurantPOSDbContext(); return db.RestaurantTables.OrderBy(t => t.Area).ThenBy(t => t.TableName).ToList(); }
        public List<Customer> GetCustomers() { using var db = new RestaurantPOSDbContext(); return db.Customers.OrderBy(c => c.FullName).ToList(); }
        public List<Reservation> GetReservations() { using var db = new RestaurantPOSDbContext(); return db.Reservations.OrderByDescending(r => r.ReservationTime).ToList(); }
        public List<StockMovement> GetStockMovements() { using var db = new RestaurantPOSDbContext(); return db.StockMovements.OrderByDescending(m => m.CreatedAt).Take(500).ToList(); }
        public List<Invoice> GetInvoices() { using var db = new RestaurantPOSDbContext(); return db.Invoices.OrderByDescending(i => i.PaidAt).ToList(); }
        public List<InvoicePrintLine> GetInvoiceLines(int sessionId)
        {
            using var db = new RestaurantPOSDbContext();
            return (from o in db.Orders join oi in db.OrderItems on o.OrderId equals oi.OrderId
                    join d in db.Dishes on oi.DishId equals d.DishId
                    where o.SessionId == sessionId && oi.Status != "cancelled"
                    select new InvoicePrintLine { DishName = d.DishName, Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice, Amount = oi.Quantity * oi.UnitPrice }).ToList();
        }
        public List<AuditLog> GetAuditLogs() { using var db = new RestaurantPOSDbContext(); return db.AuditLogs.OrderByDescending(a => a.CreatedAt).Take(500).ToList(); }

        public bool SaveEmployee(Employee employee, string? newPassword)
        {
            using var db = new RestaurantPOSDbContext();
            if (employee.EmployeeId == 0) db.Employees.Add(employee); else db.Employees.Update(employee);
            if (!string.IsNullOrWhiteSpace(newPassword)) employee.PasswordHash = Services.PasswordSecurity.Hash(newPassword);
            return db.SaveChanges() > 0;
        }

        public bool SaveCategory(Category category)
        {
            using var db = new RestaurantPOSDbContext();
            if (category.CategoryId == 0) db.Categories.Add(category); else db.Categories.Update(category);
            return db.SaveChanges() > 0;
        }

        public bool SaveDish(Dish dish)
        {
            using var db = new RestaurantPOSDbContext();
            if (dish.DishId == 0) db.Dishes.Add(dish); else db.Dishes.Update(dish);
            return db.SaveChanges() > 0;
        }

        public bool SaveTable(RestaurantTable table)
        {
            using var db = new RestaurantPOSDbContext();
            if (table.TableId == 0) db.RestaurantTables.Add(table); else db.RestaurantTables.Update(table);
            return db.SaveChanges() > 0;
        }

        public bool HasReservationConflict(int tableId, DateTime time, int exceptId)
        {
            DateTime from = time.AddHours(-2), to = time.AddHours(2);
            using var db = new RestaurantPOSDbContext();
            return db.Reservations.Any(r => r.TableId == tableId && r.ReservationId != exceptId &&
                r.Status == "confirmed" && r.ReservationTime > from && r.ReservationTime < to);
        }

        public bool SaveReservation(Reservation reservation)
        {
            using var db = new RestaurantPOSDbContext();
            if (reservation.ReservationId == 0) db.Reservations.Add(reservation); else db.Reservations.Update(reservation);
            return db.SaveChanges() > 0;
        }

        public bool CheckInReservation(int reservationId, int employeeId)
        {
            using var db = new RestaurantPOSDbContext();
            using var tx = db.Database.BeginTransaction();
            var reservation = db.Reservations.Find(reservationId);
            if (reservation == null || reservation.Status != "confirmed") return false;
            var table = db.RestaurantTables.Find(reservation.TableId);
            if (table == null || !table.IsActive || table.Status != "available") return false;
            bool alreadyOpen = (from ts in db.TableSessions join ds in db.DiningSessions on ts.SessionId equals ds.SessionId
                                where ts.TableId == table.TableId && ds.Status == "open" select ds).Any();
            if (alreadyOpen) return false;
            var session = new DiningSession { CustomerId = reservation.CustomerId, OpenedAt = DateTime.Now, OpenedByEmployeeId = employeeId, Status = "open" };
            db.DiningSessions.Add(session); db.SaveChanges();
            db.TableSessions.Add(new TableSession { TableId = table.TableId, SessionId = session.SessionId });
            table.Status = "occupied"; reservation.Status = "checked_in";
            db.SaveChanges(); tx.Commit(); return true;
        }

        public bool AdjustStock(int ingredientId, decimal signedQuantity, string type, string reason, int? employeeId)
        {
            using var db = new RestaurantPOSDbContext();
            using var tx = db.Database.BeginTransaction();
            var ingredient = db.Ingredients.Find(ingredientId);
            if (ingredient == null || signedQuantity == 0 || ingredient.StockQuantity + signedQuantity < 0) return false;
            decimal before = ingredient.StockQuantity;
            ingredient.StockQuantity += signedQuantity;
            db.StockMovements.Add(new StockMovement { IngredientId = ingredientId, MovementType = type,
                Quantity = Math.Abs(signedQuantity), QuantityBefore = before, QuantityAfter = ingredient.StockQuantity,
                Reason = reason, EmployeeId = employeeId, CreatedAt = DateTime.Now });
            db.SaveChanges(); tx.Commit(); return true;
        }

        public bool CancelInvoice(int invoiceId, int managerId, string reason)
        {
            using var db = new RestaurantPOSDbContext();
            using var tx = db.Database.BeginTransaction();
            var invoice = db.Invoices.Find(invoiceId);
            if (invoice == null || invoice.Status != "paid") return false;
            invoice.Status = "cancelled"; invoice.CancelledAt = DateTime.Now;
            invoice.CancelledByEmployeeId = managerId; invoice.CancellationReason = reason;
            db.AuditLogs.Add(new AuditLog { EmployeeId = managerId, Action = "cancel_invoice", EntityType = "invoice",
                EntityId = invoiceId, Description = reason, CreatedAt = DateTime.Now });
            var session = db.DiningSessions.Find(invoice.SessionId);
            if (session?.CustomerId != null)
            {
                var customer = db.Customers.Find(session.CustomerId.Value);
                if (customer != null)
                {
                    int pointsToReverse = (int)(invoice.TotalAmount / 10000);
                    customer.LoyaltyPoints = Math.Max(0, customer.LoyaltyPoints - pointsToReverse);
                    customer.MembershipTier = customer.LoyaltyPoints >= 1000 ? "vip_gold" : customer.LoyaltyPoints >= 500 ? "vip" : "regular";
                }
            }
            bool result = db.SaveChanges() > 0; tx.Commit(); return result;
        }

        public void AddAudit(int? employeeId, string action, string entityType, int? entityId, string description)
        {
            using var db = new RestaurantPOSDbContext();
            db.AuditLogs.Add(new AuditLog { EmployeeId = employeeId, Action = action, EntityType = entityType,
                EntityId = entityId, Description = description, CreatedAt = DateTime.Now });
            db.SaveChanges();
        }
    }

    public class InvoicePrintLine
    {
        public string DishName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
    }
}
