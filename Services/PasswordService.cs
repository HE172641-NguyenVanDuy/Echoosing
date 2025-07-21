using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public interface IPasswordService
    {
        string HashPassword(string rawData);
        bool VerifyPassword(string rawData, string hashedPassword);
    }

    public class PasswordService : IPasswordService
    {
        public string HashPassword(string rawData)
        {
            if (string.IsNullOrEmpty(rawData))
                throw new ArgumentNullException(nameof(rawData), "Input cannot be null or empty.");

            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
        public bool VerifyPassword(string rawData, string hashedPassword)
        {
            if (string.IsNullOrEmpty(rawData) || string.IsNullOrEmpty(hashedPassword))
                return false;

            string hashedInput = HashPassword(rawData);
            return hashedInput == hashedPassword;
        }
    }
}
