using System;

namespace Unisave.Exceptions
{
    [System.Serializable]
    public class PlayerRegistrationRejectedException : System.Exception
    {
        public object Payload { get; private set; }

        public PlayerRegistrationRejectedException(string message, object payload) : base(message)
        {
            Payload = payload;
        }
    }
}
