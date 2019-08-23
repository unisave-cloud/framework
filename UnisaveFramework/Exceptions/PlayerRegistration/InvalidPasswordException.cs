using System;

namespace Unisave.Exceptions.PlayerRegistration
{
    [System.Serializable]
    public class InvalidPasswordException : PlayerRegistrationException
    {
        public override string MessageForPlayer { get; protected set; }
            = "Provided password is invalid.";

        public InvalidPasswordException() { }
        public InvalidPasswordException(string message) : base(message) { }
        public InvalidPasswordException(string message, System.Exception inner) : base(message, inner) { }
        protected InvalidPasswordException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
