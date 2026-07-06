using System.Collections.Generic;
using RestaurantPOS.Models;
using RestaurantPOS.Repositories;

namespace RestaurantPOS.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;

        public PaymentService()
        {
            _paymentRepository = new PaymentRepository();
        }

        public List<PaymentDetail> GetPaymentsByInvoiceId(int invoiceId)
        {
            return _paymentRepository.GetPaymentsByInvoiceId(invoiceId);
        }
    }
}
