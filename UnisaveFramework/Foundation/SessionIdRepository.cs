using System;
using System.Security.Cryptography;

namespace Unisave.Foundation
{
    /// <summary>
    /// Stores the session id
    /// </summary>
    public class SessionIdRepository
    {
        /// <summary>
        /// The session ID
        /// </summary>
        public string SessionId { get; internal set; }
        
        /// <summary>
        /// Generates new random session id
        /// </summary>
        public static string GenerateSessionId()
        {
            using (RandomNumberGenerator rng = new RNGCryptoServiceProvider())
            {
                byte[] randomData = new byte[15];
                rng.GetBytes(randomData);

                return Convert.ToBase64String(randomData);
            }
        }
    }
}