using System;

namespace Unisave.Utils
{
    [Serializable]
    public class MethodSearchException : System.Exception
    {
        public MethodSearchException() { }
        public MethodSearchException(string message) : base(message) { }
        public MethodSearchException(string message, System.Exception inner) : base(message, inner) { }
        protected MethodSearchException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
