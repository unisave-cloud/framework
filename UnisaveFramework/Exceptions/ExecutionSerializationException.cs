using System;

namespace Unisave.Exceptions
{
    [System.Serializable]
    public class ExecutionSerializationException : System.Exception
    {
        public ExecutionSerializationException() { }
        public ExecutionSerializationException(string message) : base(message) { }
        public ExecutionSerializationException(string message, System.Exception inner) : base(message, inner) { }
        protected ExecutionSerializationException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
