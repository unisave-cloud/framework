using System;
using System.Runtime.Serialization;

namespace Unisave.Serialization.Exceptions
{
    /// <summary>
    /// Wraps an exception thrown by the backend when running on the server.
    /// This wrapping takes place only if the received exception couldn't
    /// have been deserialized.
    /// </summary>
    [Serializable]
    public class BackendException : Exception
    {
        public BackendException(SerializedException original) : base(
            "Backend threw an exception that couldn't be deserialized.",
            original
        ) { }

        protected BackendException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        { }
    }
}