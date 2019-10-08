namespace Unisave.Exceptions
{
    [System.Serializable]
    public class DatabaseDeadlockException : NetworkingException
    {
        public DatabaseDeadlockException()
            : base("Entity lock couldn't be acquired, " +
                   "it would lead to a deadlock.") { }
        public DatabaseDeadlockException(string message) : base(message) { }
        public DatabaseDeadlockException(string message, System.Exception inner) : base(message, inner) { }
        protected DatabaseDeadlockException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}