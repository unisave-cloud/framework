using System;
using System.Security.Cryptography;

namespace Unisave.Utils
{
    /// <summary>
    /// Allows you to create secure hashes
    /// </summary>
    public static class Hash
    {
        /*
         * Implementation currently uses PBKDF2 algorithm and is taken from:
         * https://stackoverflow.com/questions/4181198/how-to-hash-a-password/10402129#10402129
         *
         * EXTENDING THIS CLASS:
         * Base64 cannot include "$" symbol. So use that for storing
         * algorithms together with the hashed values. It seems that
         * PHP password_hash(...) function uses "$" as meta symbols.
         * Get inspired there.
         *
         * COOL READING:
         * https://crackstation.net/hashing-security.htm
         */

        private const int Iterations = 10_000;

        private const int SaltSize = 16;
        private const int HashSize = 20;
        
        public static string Make(string value)
        {
            byte[] salt = new byte[SaltSize];
            new RNGCryptoServiceProvider().GetBytes(salt);
            
            var pbkdf2 = new Rfc2898DeriveBytes(value, salt, Iterations);
            byte[] hash = pbkdf2.GetBytes(HashSize);
            
            byte[] hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);
            
            return Convert.ToBase64String(hashBytes);
        }

        public static bool Check(string value, string hashedValue)
        {
            byte[] hashBytes = Convert.FromBase64String(hashedValue);
            
            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);
            
            var pbkdf2 = new Rfc2898DeriveBytes(value, salt, Iterations);
            byte[] hash = pbkdf2.GetBytes(HashSize);
            
            for (int i = 0; i < HashSize; i++)
                if (hashBytes[i + SaltSize] != hash[i])
                    return false;
            
            return true;
        }
    }
}