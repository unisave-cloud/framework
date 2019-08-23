using System;

namespace Unisave.Exceptions.ServerConnection
{
    [System.Serializable]
    public class InvalidGameTokenException : ServerConnectionException
    {
        public InvalidGameTokenException() { }
        public InvalidGameTokenException(string message) : base(message) { }
        public InvalidGameTokenException(string message, System.Exception inner) : base(message, inner) { }
        protected InvalidGameTokenException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
