using System;

namespace Unisave.Exceptions
{
    [System.Serializable]
    public class InstantiationException : System.Exception
    {
        public InstantiationException() { }
        public InstantiationException(string message) : base(message) { }
        public InstantiationException(string message, System.Exception inner) : base(message, inner) { }
        protected InstantiationException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
