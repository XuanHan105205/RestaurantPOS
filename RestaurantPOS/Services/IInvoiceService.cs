using System.Collections.Generic;
using RestaurantPOS.Models;

namespace RestaurantPOS.Services
{
    public interface IInvoiceService
    {
        Invoice GetInvoiceBySessionId(int sessionId);
        bool CreateInvoiceAndCloseSession(Invoice invoice, List<PaymentDetail> payments, List<int> tableIds, string nextTableStatus, Customer? customer, int loyaltyPointsEarned);
    }
}
