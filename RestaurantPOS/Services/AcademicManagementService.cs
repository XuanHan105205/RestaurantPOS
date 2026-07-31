using RestaurantPOS.Models;
using RestaurantPOS.Repositories;

namespace RestaurantPOS.Services
{
    public class AcademicManagementService
    {
        private static readonly string[] Roles = { "manager", "waiter", "kitchen", "cashier" };
        private readonly AcademicManagementRepository _repository = new();
        private int? CurrentEmployeeId => AuthService.Instance.CurrentUser?.EmployeeId;

        public List<Employee> GetEmployees() => _repository.GetEmployees();
        public List<Category> GetCategories() => _repository.GetCategories();
        public List<Dish> GetDishes() => _repository.GetDishes();
        public List<RestaurantTable> GetTables() => _repository.GetTables();
        public List<Customer> GetCustomers() => _repository.GetCustomers();
        public List<Reservation> GetReservations() => _repository.GetReservations();
        public List<StockMovement> GetStockMovements() => _repository.GetStockMovements();
        public List<Invoice> GetInvoices() => _repository.GetInvoices();
        public List<InvoicePrintLine> GetInvoiceLines(int sessionId) => _repository.GetInvoiceLines(sessionId);
        public List<AuditLog> GetAuditLogs() => _repository.GetAuditLogs();

        public (bool Success, string Message) SaveEmployee(Employee employee, string? password)
        {
            if (!IsRole("manager")) return (false, "Chỉ manager được quản lý nhân viên.");
            if (string.IsNullOrWhiteSpace(employee.FullName) || string.IsNullOrWhiteSpace(employee.Username)) return (false, "Tên và tài khoản không được trống.");
            employee.Role = employee.Role.Trim().ToLowerInvariant();
            if (!Roles.Contains(employee.Role)) return (false, "Vai trò không hợp lệ.");
            if (employee.EmployeeId == 0 && string.IsNullOrWhiteSpace(password)) return (false, "Nhân viên mới phải có mật khẩu.");
            employee.CreatedAt = employee.CreatedAt == default ? DateTime.Now : employee.CreatedAt;
            try { bool ok = _repository.SaveEmployee(employee, password); if (ok) Audit("save", "employee", employee.EmployeeId, employee.Username); return (ok, ok ? "Đã lưu nhân viên." : "Không có thay đổi."); }
            catch { return (false, "Tài khoản có thể đã tồn tại."); }
        }

        public (bool Success, string Message) SetEmployeeActive(Employee employee, bool active)
        {
            if (!IsRole("manager")) return (false, "Chỉ manager được khóa/mở tài khoản.");
            if (employee.EmployeeId == CurrentEmployeeId && !active) return (false, "Không thể tự khóa tài khoản đang đăng nhập.");
            employee.IsActive = active;
            var result = SaveEmployee(employee, null);
            if (result.Success) Audit(active ? "unlock" : "lock", "employee", employee.EmployeeId, employee.Username);
            return result;
        }

        public (bool Success, string Message) SaveCategory(Category category)
        {
            if (!IsRole("manager")) return (false, "Chỉ manager được quản lý thực đơn.");
            if (string.IsNullOrWhiteSpace(category.CategoryName)) return (false, "Tên danh mục không được trống.");
            try { bool ok = _repository.SaveCategory(category); if (ok) Audit("save", "category", category.CategoryId, category.CategoryName); return (ok, ok ? "Đã lưu danh mục." : "Không có thay đổi."); }
            catch { return (false, "Tên danh mục đã tồn tại."); }
        }

        public (bool Success, string Message) SaveDish(Dish dish)
        {
            if (!IsRole("manager")) return (false, "Chỉ manager được quản lý thực đơn.");
            if (string.IsNullOrWhiteSpace(dish.DishName) || dish.Price <= 0 || dish.CategoryId is null or <= 0) return (false, "Tên, giá và danh mục món không hợp lệ.");
            if (dish.AvailabilityStatus is not ("active" or "discontinued")) return (false, "Trạng thái món không hợp lệ.");
            try { bool ok = _repository.SaveDish(dish); if (ok) Audit("save", "dish", dish.DishId, dish.DishName); return (ok, ok ? "Đã lưu món." : "Không có thay đổi."); }
            catch { return (false, "Không thể lưu món."); }
        }

        public (bool Success, string Message) SaveTable(RestaurantTable table)
        {
            if (!IsRole("manager")) return (false, "Chỉ manager được quản lý bàn.");
            if (string.IsNullOrWhiteSpace(table.TableName) || table.Capacity is null or <= 0) return (false, "Tên bàn và sức chứa không hợp lệ.");
            try { bool ok = _repository.SaveTable(table); if (ok) Audit("save", "table", table.TableId, table.TableName); return (ok, ok ? "Đã lưu bàn." : "Không có thay đổi."); }
            catch { return (false, "Tên bàn đã tồn tại hoặc dữ liệu không hợp lệ."); }
        }

        public (bool Success, string Message) SaveReservation(Reservation reservation)
        {
            if (!IsRole("manager", "waiter")) return (false, "Bạn không có quyền quản lý đặt bàn.");
            if (reservation.CustomerId <= 0 || reservation.TableId <= 0 || reservation.GuestCount <= 0) return (false, "Khách, bàn và số khách là bắt buộc.");
            if (reservation.ReservationTime < DateTime.Now.AddMinutes(-5)) return (false, "Thời gian đặt phải ở tương lai.");
            var table = GetTables().FirstOrDefault(t => t.TableId == reservation.TableId);
            if (table == null || !table.IsActive || reservation.GuestCount > table.Capacity) return (false, "Bàn không hoạt động hoặc không đủ sức chứa.");
            if (_repository.HasReservationConflict(reservation.TableId, reservation.ReservationTime, reservation.ReservationId)) return (false, "Bàn đã có lịch trong khoảng ±2 giờ.");
            reservation.CreatedByEmployeeId = CurrentEmployeeId ?? 0;
            try { bool ok = _repository.SaveReservation(reservation); if (ok) Audit("save", "reservation", reservation.ReservationId, $"Bàn #{reservation.TableId}"); return (ok, ok ? "Đã lưu đặt bàn." : "Không có thay đổi."); }
            catch { return (false, "Không thể lưu đặt bàn."); }
        }

        public (bool Success, string Message) SetReservationStatus(Reservation reservation, string status)
        {
            if (!IsRole("manager", "waiter")) return (false, "Bạn không có quyền quản lý đặt bàn.");
            if (status == "checked_in") return (_repository.CheckInReservation(reservation.ReservationId, CurrentEmployeeId ?? 0), "Đã nhận bàn.");
            if (status is not ("cancelled" or "no_show")) return (false, "Trạng thái không hợp lệ.");
            reservation.Status = status; bool ok = _repository.SaveReservation(reservation);
            if (ok) Audit(status, "reservation", reservation.ReservationId, reservation.Note ?? "");
            return (ok, ok ? "Đã cập nhật đặt bàn." : "Không thể cập nhật.");
        }

        public (bool Success, string Message) AdjustStock(int ingredientId, decimal quantity, bool increase, bool waste, string reason)
        {
            if (!IsRole("manager")) return (false, "Bạn không có quyền điều chỉnh kho.");
            if (ingredientId <= 0 || quantity <= 0 || string.IsNullOrWhiteSpace(reason)) return (false, "Chọn nguyên liệu, nhập số lượng và lý do.");
            string type = increase ? "adjustment_in" : waste ? "waste" : "adjustment_out";
            bool ok = _repository.AdjustStock(ingredientId, increase ? quantity : -quantity, type, reason.Trim(), CurrentEmployeeId);
            if (ok) Audit(type, "ingredient", ingredientId, reason);
            return (ok, ok ? "Đã điều chỉnh tồn kho." : "Không đủ tồn kho hoặc dữ liệu không hợp lệ.");
        }

        public (bool Success, string Message) CancelInvoice(Invoice invoice, string reason)
        {
            if (AuthService.Instance.CurrentUser?.Role != "manager") return (false, "Chỉ manager được hủy hóa đơn.");
            if (string.IsNullOrWhiteSpace(reason)) return (false, "Phải nhập lý do hủy.");
            bool ok = _repository.CancelInvoice(invoice.InvoiceId, CurrentEmployeeId ?? 0, reason.Trim());
            return (ok, ok ? "Đã hủy hóa đơn." : "Hóa đơn không tồn tại hoặc đã hủy.");
        }

        private void Audit(string action, string entity, int? id, string description)
            => _repository.AddAudit(CurrentEmployeeId, action, entity, id, description);
        private static bool IsRole(params string[] roles) => roles.Contains(AuthService.Instance.CurrentUser?.Role ?? "");
    }
}
