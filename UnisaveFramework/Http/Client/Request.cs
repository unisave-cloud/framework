using System.Net.Http;

namespace Unisave.Http.Client
{
    public class Request
    {
        /// <summary>
        /// The original request message from the .NET framework
        /// </summary>
        public HttpRequestMessage Original { get; }
        
        public Request(HttpRequestMessage original)
        {
            Original = original;
        }
    }
}