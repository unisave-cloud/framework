using System;
using System.Runtime.Serialization;

namespace Unisave.Arango.Execution
{
    [Serializable]
    public class QueryExecutionException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public QueryExecutionException()
        {
        }

        public QueryExecutionException(string message) : base(message)
        {
        }

        public QueryExecutionException(string message, Exception inner) : base(message, inner)
        {
        }

        protected QueryExecutionException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}