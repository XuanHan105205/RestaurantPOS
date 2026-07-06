using System.Collections.Generic;
using RestaurantPOS.Models;

namespace RestaurantPOS.Repositories
{
    public interface IInvoiceRepository : IBaseRepository<Invoice>
    {
        Invoice GetBySessionId(int sessionId);
        bool CreateInvoiceAndCloseSession(Invoice invoice, List<PaymentDetail> payments, List<int> tableIds, string nextTableStatus, Customer? customerToUpdate, int loyaltyPointsEarned);
    }
}
