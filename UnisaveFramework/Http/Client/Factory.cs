using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Unisave.Http.Client
{
    public class Factory
    {
        private HttpClient client;
        private InterceptingHandler handler;
        
        public Factory(HttpMessageHandler innerHandler)
        {
            handler = new InterceptingHandler(
                innerHandler
            );
            
            client = new HttpClient(handler);
        }

        /// <summary>
        /// Creates a new instance of the pending request
        /// </summary>
        public PendingRequest PendingRequest()
        {
            return new PendingRequest(client);
        }
    }

    public class InterceptingHandler : DelegatingHandler
    {
        public InterceptingHandler(HttpMessageHandler innerHandler) : base(innerHandler)
        { }
        
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var wrappedRequest = new Request(request);
            
            var response = await base.SendAsync(request, cancellationToken);
            
            var wrappedResponse = new Response(response);
            
            return response;
        }
    }
}