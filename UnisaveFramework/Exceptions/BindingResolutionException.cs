using System;
using System.Runtime.Serialization;

namespace Unisave.Exceptions
{
    [Serializable]
    public class BindingResolutionException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public BindingResolutionException()
        {
        }

        public BindingResolutionException(string message) : base(message)
        {
        }

        public BindingResolutionException(string message, Exception inner) : base(message, inner)
        {
        }

        protected BindingResolutionException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}