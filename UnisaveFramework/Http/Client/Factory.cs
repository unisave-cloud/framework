using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Unisave.Http.Client
{
    public class Factory
    {
        /// <summary>
        /// Record about a request-response pair
        /// </summary>
        public struct Record
        {
            public Request Request { get; }
            public Response Response { get; }
            
            public Record(Request request, Response response)
            {
                Request = request;
                Response = response;
            }
        }
        
        private HttpClient client;
        private InterceptingHandler handler;

        /// <summary>
        /// Contains a list of stub callbacks that will be called
        /// in order for each new request. Each callback can return
        /// a fake response to mock the request processing. When a callback
        /// returns null, the next callback will be processed. If all
        /// callbacks return null, the request will be handled as usual.
        /// </summary>
        private List<Func<Request, Response>> stubCallbacks
            = new List<Func<Request, Response>>();
        
        public Factory(HttpMessageHandler innerHandler)
        {
            handler = new InterceptingHandler(
                innerHandler
            );
            
            client = new HttpClient(handler);
        }

        public Factory Fake()
        {
            return this;
        }
        
        public Factory Fake(string urlPattern)
        {
            return this;
        }
        
        public Factory Fake(Response response)
        {
            return this;
        }
        
        public Factory Fake(string urlPattern, Response response)
        {
            return this;
        }
        
        public Factory Fake(ResponseSequence sequence)
        {
            return this;
        }
        
        public Factory Fake(string urlPattern, ResponseSequence sequence)
        {
            return this;
        }
        
        public Factory Fake(Func<Request, Response> callback)
        {
            return this;
        }
        
        public Factory Fake(string urlPattern, Func<Request, Response> callback)
        {
            return this;
        }

        public List<Record> Recorded()
        {
            return null;
        }
        
        public List<Record> Recorded(Func<Request, Response, bool> condition)
        {
            return null;
        }
        
        public List<Record> Recorded(Func<Request, bool> condition)
        {
            return null;
        }

        public void AssertSent(Func<Request, Response, bool> condition)
        {
            
        }
        
        public void AssertSent(Func<Request, bool> condition)
        {
            
        }
        
        public void AssertNotSent(Func<Request, Response, bool> condition)
        {
            
        }
        
        public void AssertNotSent(Func<Request, bool> condition)
        {
            
        }

        public void AssertNothingSent()
        {
            
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