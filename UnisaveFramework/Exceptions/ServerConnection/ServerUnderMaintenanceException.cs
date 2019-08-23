using System;
namespace Unisave.Exceptions.ServerConnection
{
    [System.Serializable]
    public class ServerUnderMaintenanceException : ServerConnectionException
    {
        public override string MessageForPlayer { get; protected set; }
            = "Server is under maintenance. Please come back later.";

        public ServerUnderMaintenanceException() { }
        public ServerUnderMaintenanceException(string message) : base(message) { }
        public ServerUnderMaintenanceException(string message, System.Exception inner) : base(message, inner) { }
        protected ServerUnderMaintenanceException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
