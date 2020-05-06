using System;
using System.Runtime.Serialization;

namespace Unisave.Exceptions
{
    /// <summary>
    /// Thrown by the asset whenever there's a problem with reaching
    /// the Unisave servers. The specific cause will be stored
    /// in the inner exception and it will probably be
    /// some WebException or IOException.
    ///
    /// You can report this exception as "Server not reachable" or
    /// "Connection lost" to the player.
    /// </summary>
    [Serializable]
    public class UnisaveConnectionException : UnisaveException
    {
        public UnisaveConnectionException() : base(
            "Failed to reach Unisave servers."
        ) { }
        
        public UnisaveConnectionException(string message) : base(message) { }
        
        public UnisaveConnectionException(string message, Exception inner)
            : base(message, inner) { }
        
        protected UnisaveConnectionException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}