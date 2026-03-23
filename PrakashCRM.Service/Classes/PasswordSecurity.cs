using System;
using System.Security.Cryptography;
using System.Web.Helpers;

namespace PrakashCRM.Service.Classes
{
    public static class PasswordSecurity
    {
        public static string ProtectPasswordForStorage(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return string.Empty;

            return EncryptDecryptClass.Encrypt(password.Trim(), true);
        }

        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return string.Empty;

            return Crypto.HashPassword(password.Trim());
        }

        public static bool VerifyPassword(string plainTextPassword, string storedPassword)
        {
            if (string.IsNullOrWhiteSpace(plainTextPassword) || string.IsNullOrWhiteSpace(storedPassword))
                return false;

            plainTextPassword = plainTextPassword.Trim();

            if (IsHashedPassword(storedPassword))
                return Crypto.VerifyHashedPassword(storedPassword, plainTextPassword);

            try
            {
                string decryptedPassword = EncryptDecryptClass.Decrypt(storedPassword, true);
                return string.Equals(decryptedPassword.Trim(), plainTextPassword, StringComparison.Ordinal);
            }
            catch
            {
                return false;
            }
        }

        public static bool IsHashedPassword(string storedPassword)
        {
            return !string.IsNullOrWhiteSpace(storedPassword)
                && storedPassword.StartsWith("AQAAAA", StringComparison.Ordinal);
        }

        public static string GenerateSecureToken(int sizeInBytes = 32)
        {
            byte[] buffer = new byte[sizeInBytes];

            using (RandomNumberGenerator random = RandomNumberGenerator.Create())
            {
                random.GetBytes(buffer);
            }

            return Convert.ToBase64String(buffer)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}