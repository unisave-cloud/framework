using System;
using Unisave.Exceptions;

namespace Unisave.Exceptions.PlayerRegistration
{
    /// <summary>
    /// When something goes wrong during player registration
    /// </summary>
    [System.Serializable]
    public class PlayerRegistrationException : ExceptionWithPlayerMessage
    {
        public PlayerRegistrationException() { }
        public PlayerRegistrationException(string message) : base(message) { }
        public PlayerRegistrationException(string message, System.Exception inner) : base(message, inner) { }
        protected PlayerRegistrationException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
