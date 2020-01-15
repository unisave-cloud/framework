using System;
using System.Runtime.Serialization;
using Unisave.Exceptions;

namespace Unisave.Modules.Matchmaking.Exceptions
{
    [Serializable]
    public class UnknownPlayerPollingException : UnisaveException
    {
        public UnknownPlayerPollingException()
        {
        }

        public UnknownPlayerPollingException(string message) : base(message)
        {
        }

        public UnknownPlayerPollingException(string message, Exception inner) : base(message, inner)
        {
        }

        protected UnknownPlayerPollingException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}