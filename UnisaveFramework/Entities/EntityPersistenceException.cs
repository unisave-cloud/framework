using System;
using System.Runtime.Serialization;

namespace Unisave.Entities
{
    [Serializable]
    public class EntityPersistenceException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public EntityPersistenceException()
        {
        }

        public EntityPersistenceException(string message) : base(message)
        {
        }

        public EntityPersistenceException(string message, Exception inner) : base(message, inner)
        {
        }

        protected EntityPersistenceException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}