using System.Collections.Generic;
using RestaurantPOS.Models;

namespace RestaurantPOS.Repositories
{
    public interface IPaymentRepository : IBaseRepository<PaymentDetail>
    {
        List<PaymentDetail> GetPaymentsByInvoiceId(int invoiceId);
    }
}
