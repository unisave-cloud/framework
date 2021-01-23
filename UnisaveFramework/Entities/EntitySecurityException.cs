using System;
using System.Runtime.Serialization;

namespace Unisave.Entities
{
    /// <summary>
    /// Thrown when the user tries to perform an insecure
    /// operation regarding entities
    /// </summary>
    [Serializable]
    public class EntitySecurityException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public EntitySecurityException()
        {
        }

        public EntitySecurityException(string message) : base(message)
        {
        }

        public EntitySecurityException(string message, Exception inner) : base(message, inner)
        {
        }

        protected EntitySecurityException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}