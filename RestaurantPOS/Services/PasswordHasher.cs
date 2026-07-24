using System;
using System.Security.Cryptography;
using System.Text;

namespace RestaurantPOS.Services
{
    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return string.Empty;

            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        public static bool VerifyPassword(string inputPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(inputPassword) || string.IsNullOrEmpty(storedHash))
            {
                return false;
            }

            // Fallback hỗ trợ dữ liệu seed ban đầu (mật khẩu chưa băm "123456")
            if (storedHash.Equals(inputPassword, StringComparison.Ordinal))
            {
                return true;
            }

            // Hỗ trợ kiểm tra mật khẩu đã băm SHA-256
            string inputHash = HashPassword(inputPassword);
            return string.Equals(inputHash, storedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
