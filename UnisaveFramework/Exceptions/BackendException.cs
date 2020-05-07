using System;
using System.Runtime.Serialization;
using Unisave.Serialization.Exceptions;

namespace Unisave.Exceptions
{
    /// <summary>
    /// Wraps an exception thrown by the backend when running on the server.
    /// This wrapping takes place only if the received exception couldn't
    /// have been deserialized.
    ///
    /// This exception is not a UnisaveException, because the wrapped exception
    /// may be a user-defined one, so it has nothing to do with Unisave.
    /// In a perfect world with people using Unisave and C# properly
    /// and Unisave having no bugs would this exception never appear.
    /// It's a failsafe to prevent a non-deserializable exception
    /// from being silenced.
    /// </summary>
    [Serializable]
    public sealed class BackendException : Exception
    {
        public BackendException() : base(
            "Backend threw an exception, but no more information was " +
            "extracted about it."
        ) { }
        
        public BackendException(SerializedException original) : base(
            "Backend threw an exception that couldn't be deserialized.",
            original
        ) { }

        private BackendException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        { }
    }
}