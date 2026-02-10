using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace COURSEPROJECT
{
    public static class HashCode
    {
        public static string HashPassword(string password)
        {
            string salt = "course2025";
            using (var sha256 = SHA256.Create())
            {
                var saltedPassword = password + salt;
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltedPassword));
                return Convert.ToBase64String(bytes);
            }
        }   
        public static bool VerifyPassword(string enteredPassword, string storedHash)
        {
            var hashOfEntered = HashPassword(enteredPassword);
            return hashOfEntered == storedHash;
        }
        public static bool IsHash(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            try
            {
                return input.Length == 44 && Convert.FromBase64String(input) != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
