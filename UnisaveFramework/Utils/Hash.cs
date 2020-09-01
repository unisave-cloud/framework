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
            // decompose hashed value
            
            byte[] hashedValueBytes = Convert.FromBase64String(hashedValue);
            
            byte[] hashedValueSalt = new byte[SaltSize];
            Array.Copy(hashedValueBytes, 0, hashedValueSalt, 0, SaltSize);
            
            byte[] hashedValueHash = new byte[HashSize];
            Array.Copy(hashedValueBytes, SaltSize, hashedValueHash, 0, HashSize);
            
            // hash given value
            
            var pbkdf2 = new Rfc2898DeriveBytes(value, hashedValueSalt, Iterations);
            byte[] givenValueHash = pbkdf2.GetBytes(HashSize);

            // compare the hashed given value to the given hash
            
            return SlowEquals(hashedValueHash, givenValueHash);
        }

        /// <summary>
        /// Length-constant time byte array comparison
        /// https://crackstation.net/hashing-security.htm
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        internal static bool SlowEquals(byte[] a, byte[] b)
        {
            int diff = a.Length ^ b.Length;

            for (int i = 0; i < a.Length && i < b.Length; i++)
                diff |= a[i] ^ b[i];
            
            return diff == 0;
        }
    }
}