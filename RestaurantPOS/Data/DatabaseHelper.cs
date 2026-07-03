using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;

namespace RestaurantPOS.Data
{
    public static class DatabaseHelper
    {
        private static readonly string ConnectionString = 
            ConfigurationManager.ConnectionStrings["RestaurantPOSConnection"]?.ConnectionString 
            ?? "Server=localhost;Database=RestaurantPOS;Trusted_Connection=True;TrustServerCertificate=True;";

        public static SqlConnection GetConnection()
        {
            var conn = new SqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        public static int ExecuteNonQuery(string sql, params SqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddRange(parameters);
                }
                return cmd.ExecuteNonQuery();
            }
        }

        public static object ExecuteScalar(string sql, params SqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddRange(parameters);
                }
                return cmd.ExecuteScalar();
            }
        }

        public static List<T> ExecuteReader<T>(string sql, Func<SqlDataReader, T> mapRow, params SqlParameter[] parameters)
        {
            var list = new List<T>();
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddRange(parameters);
                }
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(mapRow(reader));
                    }
                }
            }
            return list;
        }

        public static T ExecuteSingle<T>(string sql, Func<SqlDataReader, T> mapRow, params SqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (parameters != null)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddRange(parameters);
                }
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return mapRow(reader);
                    }
                }
            }
            return default;
        }

        public static T GetValue<T>(object value)
        {
            if (value == null || value == DBNull.Value)
            {
                return default;
            }
            
            Type targetType = typeof(T);
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                targetType = Nullable.GetUnderlyingType(targetType);
            }
            
            return (T)Convert.ChangeType(value, targetType);
        }

        public static object ToDbValue(object value)
        {
            return value ?? DBNull.Value;
        }
    }
}
