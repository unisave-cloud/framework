using System;
using System.Runtime.Serialization;

namespace Unisave.Serialization
{
    /// <summary>
    /// Thrown when the user attempts insecure deserialization
    /// </summary>
    [Serializable]
    public class InsecureDeserializationException : UnisaveSerializationException
    {
        public InsecureDeserializationException() { }

        public InsecureDeserializationException(string message) : base(message) { }

        public InsecureDeserializationException(string message, Exception inner)
            : base(message, inner) { }

        protected InsecureDeserializationException(
            SerializationInfo info,
            StreamingContext context
        ) : base(info, context) { }
    }
}