using RestaurantPOS.Repositories;

namespace RestaurantPOS.Services
{
    public static class AuditTrail
    {
        public static void Record(string action, string entityType, int? entityId,
            string description, int? employeeId = null)
        {
            int? actorId = employeeId ?? AuthService.Instance.CurrentUser?.EmployeeId;
            if (!actorId.HasValue) return;
            try
            {
                new AcademicManagementRepository().AddAudit(
                    actorId, action, entityType, entityId, description);
            }
            catch
            {
                // Audit không được làm hỏng nghiệp vụ chính đã thành công.
            }
        }
    }
}
