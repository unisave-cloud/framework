using System.Net.Http;

namespace Unisave.Http.Client
{
    public class Response
    {
        /// <summary>
        /// The original response message from the .NET framework
        /// </summary>
        public HttpResponseMessage Original { get; }
        
        public Response(HttpResponseMessage original)
        {
            Original = original;
        }
    }
}