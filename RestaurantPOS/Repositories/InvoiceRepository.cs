using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RestaurantPOS.Models;
using RestaurantPOS.Data;

namespace RestaurantPOS.Repositories
{
    public class InvoiceRepository : BaseRepository<Invoice>, IInvoiceRepository
    {
        public override List<Invoice> GetAll()
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.Invoices.ToList();
            }
        }

        public override Invoice GetById(int id)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.Invoices.Find(id);
            }
        }

        public override bool Add(Invoice entity)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                context.Invoices.Add(entity);
                return context.SaveChanges() > 0;
            }
        }

        public override bool Update(Invoice entity)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                context.Invoices.Update(entity);
                return context.SaveChanges() > 0;
            }
        }

        public override bool Delete(int id)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                var invoice = context.Invoices.Find(id);
                if (invoice != null)
                {
                    context.Invoices.Remove(invoice);
                    return context.SaveChanges() > 0;
                }
                return false;
            }
        }

        public Invoice GetBySessionId(int sessionId)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                return context.Invoices.FirstOrDefault(i => i.SessionId == sessionId);
            }
        }

        public bool CreateInvoiceAndCloseSession(
            Invoice invoice, 
            List<PaymentDetail> payments, 
            List<int> tableIds, 
            string nextTableStatus, 
            Customer? customerToUpdate, 
            int loyaltyPointsEarned)
        {
            using (var context = new RestaurantPOSDbContext())
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        // 1. Add Invoice
                        invoice.PaidAt = DateTime.Now;
                        context.Invoices.Add(invoice);
                        context.SaveChanges(); // to generate InvoiceId

                        // 2. Add Payment Details
                        foreach (var payment in payments)
                        {
                            payment.InvoiceId = invoice.InvoiceId;
                            context.PaymentDetails.Add(payment);
                        }

                        // 3. Close Dining Session
                        var session = context.DiningSessions.Find(invoice.SessionId);
                        if (session != null)
                        {
                            session.Status = "closed";
                            session.ClosedAt = DateTime.Now;
                            context.DiningSessions.Update(session);
                        }

                        // 4. Update Tables Status
                        foreach (var tableId in tableIds)
                        {
                            var table = context.RestaurantTables.Find(tableId);
                            if (table != null)
                            {
                                table.Status = nextTableStatus;
                                context.RestaurantTables.Update(table);
                            }
                        }

                        // 5. Update Customer Loyalty Points & Membership Tier
                        if (customerToUpdate != null)
                        {
                            var dbCustomer = context.Customers.Find(customerToUpdate.CustomerId);
                            if (dbCustomer != null)
                            {
                                dbCustomer.LoyaltyPoints += loyaltyPointsEarned;
                                
                                // Update membership tier based on points
                                if (dbCustomer.LoyaltyPoints >= 1000)
                                {
                                    dbCustomer.MembershipTier = "vip_gold";
                                }
                                else if (dbCustomer.LoyaltyPoints >= 500)
                                {
                                    dbCustomer.MembershipTier = "vip";
                                }
                                else
                                {
                                    dbCustomer.MembershipTier = "regular";
                                }
                                 context.Customers.Update(dbCustomer);
                            }
                        }

                        // 6. Tự động trừ kho cho bất kỳ món nào chưa nấu xong (vẫn ở trạng thái pending hoặc cooking)
                        var ordersInSession = context.Orders.Where(o => o.SessionId == invoice.SessionId).ToList();
                        var ingredientRepo = new IngredientRepository();
                        foreach (var order in ordersInSession)
                        {
                            var undeductedItems = context.OrderItems
                                .Where(oi => oi.OrderId == order.OrderId && (oi.Status == "pending" || oi.Status == "cooking"))
                                .ToList();

                            foreach (var oi in undeductedItems)
                            {
                                ingredientRepo.DeductStockForDish(oi.DishId, oi.Quantity, context);
                                oi.Status = "served";
                                oi.StatusUpdatedAt = DateTime.Now;
                                context.OrderItems.Update(oi);
                            }
                        }

                        context.SaveChanges();
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }
    }
}
