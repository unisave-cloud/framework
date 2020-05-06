using System;
using System.Runtime.Serialization;

namespace Unisave.Exceptions
{
    /// <summary>
    /// Indicates abnormal situation of the Unisave system.
    /// This abnormality may be caused by improper usage of the system,
    /// or by an internal error.
    /// </summary>
    [Serializable]
    public class UnisaveException : Exception
    {
        public UnisaveException() { }
        
        public UnisaveException(string message) : base(message) { }
        
        public UnisaveException(string message, Exception inner)
            : base(message, inner) { }
        
        protected UnisaveException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
