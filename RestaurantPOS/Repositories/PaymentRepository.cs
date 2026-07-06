using System.Collections.Generic;
using System.Linq;
using RestaurantPOS.Models;
using RestaurantPOS.Data;

namespace RestaurantPOS.Repositories
{
    public class PaymentRepository : BaseRepository<PaymentDetail>, IPaymentRepository
    {
        public override List<PaymentDetail> GetAll()
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.PaymentDetails.ToList();
            }
        }

        public override PaymentDetail GetById(int id)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.PaymentDetails.Find(id);
            }
        }

        public override bool Add(PaymentDetail entity)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                context.PaymentDetails.Add(entity);
                return context.SaveChanges() > 0;
            }
        }

        public override bool Update(PaymentDetail entity)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                context.PaymentDetails.Update(entity);
                return context.SaveChanges() > 0;
            }
        }

        public override bool Delete(int id)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                var payment = context.PaymentDetails.Find(id);
                if (payment != null)
                {
                    context.PaymentDetails.Remove(payment);
                    return context.SaveChanges() > 0;
                }
                return false;
            }
        }

        public List<PaymentDetail> GetPaymentsByInvoiceId(int invoiceId)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.PaymentDetails.Where(pd => pd.InvoiceId == invoiceId).ToList();
            }
        }
    }
}
