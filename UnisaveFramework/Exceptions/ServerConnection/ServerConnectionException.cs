using System;
using Unisave.Exceptions;

namespace Unisave.Exceptions.ServerConnection
{
    /// <summary>
    /// Client uses this exception to indicate problems with connection to the server
    /// </summary>
    [System.Serializable]
    public class ServerConnectionException : ExceptionWithPlayerMessage
    {
        public ServerConnectionException() { }
        public ServerConnectionException(string message) : base(message) { }
        public ServerConnectionException(string message, System.Exception inner) : base(message, inner) { }
        protected ServerConnectionException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
