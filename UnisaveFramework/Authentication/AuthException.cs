using System;
using System.Runtime.Serialization;

namespace Unisave.Authentication
{
    /// <summary>
    /// Thrown when the player is not authenticated, and needs to be, in order
    /// to perform a given action, or is not authorized to perform that action.
    /// </summary>
    [Serializable]
    public class AuthException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public AuthException()
        {
        }

        public AuthException(string message) : base(message)
        {
        }

        public AuthException(string message, Exception inner) : base(message, inner)
        {
        }

        protected AuthException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}