using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using RestaurantPOS.Models;
using RestaurantPOS.Data;

namespace RestaurantPOS.Repositories
{
    public class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
    {
        public override List<Customer> GetAll()
        {
            string sql = "SELECT customer_id, full_name, phone, membership_tier, loyalty_points FROM customers";
            return DatabaseHelper.ExecuteReader(sql, MapRow);
        }

        public override Customer GetById(int id)
        {
            string sql = "SELECT customer_id, full_name, phone, membership_tier, loyalty_points FROM customers WHERE customer_id = @Id";
            var param = new SqlParameter("@Id", id);
            return DatabaseHelper.ExecuteSingle(sql, MapRow, param);
        }

        public Customer GetByPhone(string phone)
        {
            string sql = "SELECT customer_id, full_name, phone, membership_tier, loyalty_points FROM customers WHERE phone = @Phone";
            var param = new SqlParameter("@Phone", phone);
            return DatabaseHelper.ExecuteSingle(sql, MapRow, param);
        }

        public override bool Add(Customer entity)
        {
            string sql = "INSERT INTO customers (full_name, phone, membership_tier, loyalty_points) VALUES (@FullName, @Phone, @MembershipTier, @LoyaltyPoints)";
            var p1 = new SqlParameter("@FullName", entity.FullName);
            var p2 = new SqlParameter("@Phone", DatabaseHelper.ToDbValue(entity.Phone));
            var p3 = new SqlParameter("@MembershipTier", entity.MembershipTier ?? "regular");
            var p4 = new SqlParameter("@LoyaltyPoints", entity.LoyaltyPoints);

            return DatabaseHelper.ExecuteNonQuery(sql, p1, p2, p3, p4) > 0;
        }

        public override bool Update(Customer entity)
        {
            string sql = "UPDATE customers SET full_name = @FullName, phone = @Phone, membership_tier = @MembershipTier, loyalty_points = @LoyaltyPoints WHERE customer_id = @Id";
            var p1 = new SqlParameter("@FullName", entity.FullName);
            var p2 = new SqlParameter("@Phone", DatabaseHelper.ToDbValue(entity.Phone));
            var p3 = new SqlParameter("@MembershipTier", entity.MembershipTier);
            var p4 = new SqlParameter("@LoyaltyPoints", entity.LoyaltyPoints);
            var p5 = new SqlParameter("@Id", entity.CustomerId);

            return DatabaseHelper.ExecuteNonQuery(sql, p1, p2, p3, p4, p5) > 0;
        }

        public override bool Delete(int id)
        {
            string sql = "DELETE FROM customers WHERE customer_id = @Id";
            var param = new SqlParameter("@Id", id);
            return DatabaseHelper.ExecuteNonQuery(sql, param) > 0;
        }

        private Customer MapRow(SqlDataReader reader)
        {
            return new Customer
            {
                CustomerId = DatabaseHelper.GetValue<int>(reader["customer_id"]),
                FullName = DatabaseHelper.GetValue<string>(reader["full_name"]),
                Phone = DatabaseHelper.GetValue<string>(reader["phone"]),
                MembershipTier = DatabaseHelper.GetValue<string>(reader["membership_tier"]),
                LoyaltyPoints = DatabaseHelper.GetValue<int>(reader["loyalty_points"])
            };
        }
    }
}
