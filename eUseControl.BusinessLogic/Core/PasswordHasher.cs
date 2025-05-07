using System;
using System.Security.Cryptography;
using System.Text;

namespace eUseControl.BusinessLogic.Core
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16; 
        private const int HashSize = 20; 
        private const int Iterations = 10000;

        private static readonly string GlobalPepper = "DriveNow";

        public static string HashPassword(string password)
        {
            byte[] salt = new byte[SaltSize];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            string pepperedPassword = password + GlobalPepper;

            byte[] hash = HashPasswordWithSalt(pepperedPassword, salt);

            byte[] hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            return Convert.ToBase64String(hashBytes);
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                byte[] hashBytes = Convert.FromBase64String(hashedPassword);

                byte[] salt = new byte[SaltSize];
                Array.Copy(hashBytes, 0, salt, 0, SaltSize);

                string pepperedPassword = password + GlobalPepper;

                byte[] hash = HashPasswordWithSalt(pepperedPassword, salt);

                for (int i = 0; i < HashSize; i++)
                {
                    if (hashBytes[i + SaltSize] != hash[i])
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static byte[] HashPasswordWithSalt(string password, byte[] salt)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                return pbkdf2.GetBytes(HashSize);
            }
        }
    }
}