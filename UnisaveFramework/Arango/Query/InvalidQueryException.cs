using System;
using System.Runtime.Serialization;

namespace Unisave.Arango.Query
{
    /// <summary>
    /// The query that's being constructed is invalid in some way
    /// </summary>
    [Serializable]
    public class InvalidQueryException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public InvalidQueryException()
        {
        }

        public InvalidQueryException(string message) : base(message)
        {
        }

        public InvalidQueryException(string message, Exception inner) : base(message, inner)
        {
        }

        protected InvalidQueryException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}