using System;
using System.Runtime.Serialization;

namespace Unisave.Exceptions
{
    /// <summary>
    /// Thrown when a testing assertion fails. This exception exists,
    /// because Unisave Framework does not reference NUnit framework
    /// but yet provides some assertion methods.
    /// </summary>
    [Serializable]
    public class UnisaveAssertionException : Exception
    {
        public UnisaveAssertionException() : base(
            "A Unisave assertion failed. This refers to testing " +
            "assertions, e.g. Http.AssertSent(...), not internal Unisave " +
            "debugging assertions."
        ) { }

        public UnisaveAssertionException(string message) : base(message) { }

        public UnisaveAssertionException(string message, Exception inner)
            : base(message, inner) { }

        protected UnisaveAssertionException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        { }
    }
}