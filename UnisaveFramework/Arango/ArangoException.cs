using System;
using System.Runtime.Serialization;

namespace Unisave.Arango
{
    /// <summary>
    /// Exception thrown by the arango database itself,
    /// just wrapped as a C# exception.
    /// </summary>
    [Serializable]
    public class ArangoException : Exception
    {
        /// <summary>
        /// HTTP response status
        /// </summary>
        public int HttpStatus { get; }
        
        /// <summary>
        /// Error number, see:
        /// https://www.arangodb.com/docs/stable/appendix-error-codes.html
        /// </summary>
        public int ErrorNumber { get; }
        
        /// <summary>
        /// Human-readable message regarding the error
        /// </summary>
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
            HttpStatus = info.GetInt32("HttpStatus");
            ErrorNumber = info.GetInt32("ErrorNumber");
            ErrorMessage = info.GetString("ErrorMessage");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            
            info.AddValue("HttpStatus", HttpStatus);
            info.AddValue("ErrorNumber", ErrorNumber);
            info.AddValue("ErrorMessage", ErrorMessage);
            
            base.GetObjectData(info, context);
        }
    }
}