using System;

namespace Unisave.Exceptions
{
    [System.Serializable]
    public class ConnectionEndedException : NetworkingException
    {
        public ConnectionEndedException() { }
        public ConnectionEndedException(string message) : base(message) { }
        public ConnectionEndedException(string message, System.Exception inner) : base(message, inner) { }
        protected ConnectionEndedException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
