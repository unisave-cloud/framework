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

                string text = Convert.ToBase64String(randomData);

                // ArangoDB cannot have '/' in document key so lets
                // replace it by '-' as a safe alternative.
                // (the only reason I use base64 is to make the string
                // look nice, precisely so that it could be used like this)
                return text.Replace('/', '-');
            }
        }
    }
}