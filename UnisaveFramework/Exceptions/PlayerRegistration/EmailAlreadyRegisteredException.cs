using System;

namespace Unisave.Exceptions.PlayerRegistration
{
    [System.Serializable]
    public class EmailAlreadyRegisteredException : PlayerRegistrationException
    {
        public override string MessageForPlayer { get; protected set; }
            = "This email address is already registered.";

        public EmailAlreadyRegisteredException() { }
        public EmailAlreadyRegisteredException(string message) : base(message) { }
        public EmailAlreadyRegisteredException(string message, System.Exception inner) : base(message, inner) { }
        protected EmailAlreadyRegisteredException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
