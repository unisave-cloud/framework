using System;

namespace Unisave.Exceptions.PlayerRegistration
{
    [System.Serializable]
    public class InvalidEmailException : PlayerRegistrationException
    {
        public override string MessageForPlayer { get; protected set; }
            = "Provided email is not a valid email address.";
        public InvalidEmailException() { }
        public InvalidEmailException(string message) : base(message) { }
        public InvalidEmailException(string message, System.Exception inner) : base(message, inner) { }
        protected InvalidEmailException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
