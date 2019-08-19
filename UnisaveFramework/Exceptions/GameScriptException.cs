using System;

namespace Unisave.Exceptions
{
    /// <summary>
    /// Wrapper around an exception throw by the game code
    /// </summary>
    [System.Serializable]
    public class GameScriptException : System.Exception
    {
        public GameScriptException() { }
        public GameScriptException(string message) : base(message) { }
        public GameScriptException(string message, System.Exception inner) : base(message, inner) { }
        public GameScriptException(System.Exception inner) : base("Game script threw an exception.", inner) { }
        protected GameScriptException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
