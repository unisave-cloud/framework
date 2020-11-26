using System;
using System.Security.Cryptography;

namespace Unisave.Sessions
{
    /// <summary>
    /// Stores the session id on the server-side
    /// </summary>
    public class ServerSessionIdRepository
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