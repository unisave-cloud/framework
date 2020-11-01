using System;
using System.Runtime.Serialization;

namespace Unisave.Serialization
{
    /// <summary>
    /// Thrown when Unisave serialization fails
    /// </summary>
    [Serializable]
    public class UnisaveSerializationException : Exception
    {
        public UnisaveSerializationException() { }

        public UnisaveSerializationException(string message) : base(message) { }

        public UnisaveSerializationException(string message, Exception inner)
            : base(message, inner) { }

        protected UnisaveSerializationException(
            SerializationInfo info,
            StreamingContext context
        ) : base(info, context) { }
    }
}