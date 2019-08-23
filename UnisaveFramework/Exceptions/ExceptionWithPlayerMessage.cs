using System;

namespace Unisave.Exceptions
{
    /// <summary>
    /// Exception that contains a message for the player
    /// Useful when handling exceptions inside user interface
    /// </summary>
    [System.Serializable]
    public abstract class ExceptionWithPlayerMessage : System.Exception
    {
        /// <summary>
        /// Message to be displayed to the player
        /// </summary>
        public virtual string MessageForPlayer { get; protected set; }
            = "Something went wrong.";

        public ExceptionWithPlayerMessage() { }
        public ExceptionWithPlayerMessage(string message) : base(message) { }
        public ExceptionWithPlayerMessage(string message, System.Exception inner) : base(message, inner) { }
        protected ExceptionWithPlayerMessage(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        /// <summary>
        /// Chainable method for setting player message
        /// </summary>
        public ExceptionWithPlayerMessage TellPlayer(string messageForPlayer)
        {
            MessageForPlayer = messageForPlayer;
            return this;
        }
    }
}
