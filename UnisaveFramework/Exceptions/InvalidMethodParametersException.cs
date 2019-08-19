using System;
namespace Unisave.Exceptions
{
    /// <summary>
    /// Thrown by the server script bootstrapping process when something goes wrong
    /// during locating the proper method to be called
    /// </summary>
    [System.Serializable]
    public class InvalidMethodParametersException : System.Exception
    {
        public InvalidMethodParametersException() { }
        public InvalidMethodParametersException(string message) : base(message) { }
        public InvalidMethodParametersException(string message, System.Exception inner) : base(message, inner) { }
        public InvalidMethodParametersException(System.Exception inner) : base("Script execution failed.", inner) { }
        protected InvalidMethodParametersException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
