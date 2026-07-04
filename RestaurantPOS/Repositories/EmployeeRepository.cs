using System.Collections.Generic;
using RestaurantPOS.Models;
using RestaurantPOS.Data;
using Microsoft.Data.SqlClient;

namespace RestaurantPOS.Repositories
{
    public class EmployeeRepository : BaseRepository<Employee>, IEmployeeRepository
    {
        public override List<Employee> GetAll()
        {
            // TODO: Hàn will write SQL select query here.
            return new List<Employee>();
        }

        public override Employee GetById(int id)
        {
            // TODO: Hàn will write SQL select query here.
            return null;
        }

        public Employee GetByUsername(string username)
        {
            string sql = "SELECT employee_id, full_name, username, password_hash, role, phone, is_active FROM employees WHERE username = @Username";
            var param = new SqlParameter("@Username", username);
            return DatabaseHelper.ExecuteSingle(sql, reader => new Employee
            {
                EmployeeId = DatabaseHelper.GetValue<int>(reader["employee_id"]),
                FullName = DatabaseHelper.GetValue<string>(reader["full_name"]),
                Username = DatabaseHelper.GetValue<string>(reader["username"]),
                PasswordHash = DatabaseHelper.GetValue<string>(reader["password_hash"]),
                Role = DatabaseHelper.GetValue<string>(reader["role"]),
                Phone = DatabaseHelper.GetValue<string>(reader["phone"]),
                IsActive = DatabaseHelper.GetValue<bool>(reader["is_active"])
            }, param);
        }
        public override bool Add(Employee entity)
        {
            // TODO: Hàn will write SQL insert query here.
            return false;
        }

        public override bool Update(Employee entity)
        {
            // TODO: Hàn will write SQL update query here.
            return false;
        }

        public override bool Delete(int id)
        {
            // TODO: Hàn will write SQL delete query here.
            return false;
        }
    }
}
