using System;
using System.Runtime.Serialization;

namespace Unisave.Arango.Expressions
{
    /// <summary>
    /// There was some problem when parsing a LINQ expression tree
    /// and converting it to an AQL expression
    /// </summary>
    [Serializable]
    public class AqlParsingException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public AqlParsingException()
        {
        }

        public AqlParsingException(string message) : base(message)
        {
        }

        public AqlParsingException(string message, Exception inner) : base(message, inner)
        {
        }

        protected AqlParsingException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}