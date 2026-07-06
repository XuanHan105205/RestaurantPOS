using System.Collections.Generic;
using RestaurantPOS.Models;

namespace RestaurantPOS.Services
{
    public interface IPaymentService
    {
        List<PaymentDetail> GetPaymentsByInvoiceId(int invoiceId);
    }
}
