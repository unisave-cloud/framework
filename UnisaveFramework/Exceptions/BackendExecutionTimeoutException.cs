using System;
using System.Runtime.Serialization;

namespace Unisave.Exceptions
{
    /// <summary>
    /// Thrown by the Unisave server when a backend execution
    /// ran for too long and was killed.
    ///
    /// This usually occurs when you:
    /// - accidentally make an infinite loop
    /// - make a database request that is too time-consuming
    /// - allocate a lot of memory (sometimes instead of OOM the program freezes)
    /// - perform time-consuming computations
    /// - wait for some external condition (http request, external lock, ...)
    /// </summary>
    [Serializable]
    public sealed class BackendExecutionTimeoutException : UnisaveException
    {
        public BackendExecutionTimeoutException() : base(
            "Backend execution ran for too long and was killed by Unisave.\n" +
            "Make sure you haven't accidentally created an infinite loop, " +
            "complex database request or infinite wait.\n" +
            "This might also be caused by allocating a lot of memory - " +
            "sometimes before throwing System.OutOfMemoryException " +
            "the program slows down so much that it runs past the timeout."
        ) { }

        public BackendExecutionTimeoutException(string message) : base(message)
        {
        }

        public BackendExecutionTimeoutException(string message, Exception inner) : base(message, inner)
        {
        }

        protected BackendExecutionTimeoutException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}