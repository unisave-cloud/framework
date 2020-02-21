using System;
using System.Runtime.Serialization;

namespace Unisave.Entities
{
    [Serializable]
    public class EntitySearchException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public EntitySearchException()
        {
        }

        public EntitySearchException(string message) : base(message)
        {
        }

        public EntitySearchException(string message, Exception inner) : base(message, inner)
        {
        }

        protected EntitySearchException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}