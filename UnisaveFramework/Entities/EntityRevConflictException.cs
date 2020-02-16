using System;
using System.Runtime.Serialization;

namespace Unisave.Entities
{
    [Serializable]
    public class EntityRevConflictException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public EntityRevConflictException()
        {
        }

        public EntityRevConflictException(string message) : base(message)
        {
        }

        public EntityRevConflictException(string message, Exception inner) : base(message, inner)
        {
        }

        protected EntityRevConflictException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}