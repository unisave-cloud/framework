using System;
using System.Runtime.Serialization;

namespace Unisave.Arango
{
    /// <summary>
    /// Exception thrown by the arango database itself
    /// </summary>
    [Serializable]
    public class ArangoException : Exception
    {
        public int HttpStatus { get; }
        public int ErrorNumber { get; }
        public string ErrorMessage { get; }
        
        public ArangoException(int httpStatus, int errorNumber, string errorMessage)
            : this($"[HTTP {httpStatus}] [ERROR {errorNumber}] {errorMessage}")
        {
            HttpStatus = httpStatus;
            ErrorNumber = errorNumber;
            ErrorMessage = errorMessage;
        }
        
        public ArangoException()
        {
        }

        public ArangoException(string message) : base(message)
        {
        }

        public ArangoException(string message, Exception inner) : base(message, inner)
        {
        }

        protected ArangoException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}