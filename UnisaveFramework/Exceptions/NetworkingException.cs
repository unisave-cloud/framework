using System;

namespace Unisave.Exceptions
{
    [System.Serializable]
    public class NetworkingException : UnisaveException
    {
        public NetworkingException() { }
        public NetworkingException(string message) : base(message) { }
        public NetworkingException(string message, System.Exception inner) : base(message, inner) { }
        protected NetworkingException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
