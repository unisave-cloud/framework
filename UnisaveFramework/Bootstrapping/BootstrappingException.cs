using System;
using System.Runtime.Serialization;

namespace Unisave.Bootstrapping
{
    /// <summary>
    /// Thrown when the Unisave Framework bootstrapping fails
    /// </summary>
    [Serializable]
    public class BootstrappingException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public BootstrappingException()
        {
        }

        public BootstrappingException(string message) : base(message)
        {
        }

        public BootstrappingException(string message, Exception inner) : base(message, inner)
        {
        }

        protected BootstrappingException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}