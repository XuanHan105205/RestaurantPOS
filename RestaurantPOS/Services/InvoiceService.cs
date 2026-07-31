using System.Collections.Generic;
using RestaurantPOS.Models;
using RestaurantPOS.Repositories;

namespace RestaurantPOS.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public InvoiceService()
        {
            _invoiceRepository = new InvoiceRepository();
        }

        public Invoice? GetInvoiceBySessionId(int sessionId)
        {
            return _invoiceRepository.GetBySessionId(sessionId);
        }

        public bool CreateInvoiceAndCloseSession(
            Invoice invoice, 
            List<PaymentDetail> payments, 
            List<int> tableIds, 
            string nextTableStatus, 
            Customer? customer, 
            int loyaltyPointsEarned)
        {
            bool success = _invoiceRepository.CreateInvoiceAndCloseSession(
                invoice, 
                payments, 
                tableIds, 
                nextTableStatus, 
                customer, 
                loyaltyPointsEarned);
            if (success)
                AuditTrail.Record("checkout", "invoice", invoice.InvoiceId,
                    $"Thanh toán {invoice.InvoiceNumber}, tổng tiền {invoice.TotalAmount:N0}đ.", invoice.CashierEmployeeId);
            return success;
        }
    }
}
