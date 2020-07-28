using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Unisave.Utils;

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
        
        /// <summary>
        /// Contains a list of stub callbacks that will be called
        /// in order for each new request. Each callback can return
        /// a fake response to mock the request processing. When a callback
        /// returns null, the next callback will be processed. If all
        /// callbacks return null, the request will be handled as usual.
        /// </summary>
        private readonly List<Func<Request, Response>> stubCallbacks
            = new List<Func<Request, Response>>();

        /// <summary>
        /// Are requests being recorded
        /// </summary>
        public bool IsRecording { get; private set; } = false;
        
        /// <summary>
        /// The HttpClient instance that should be used for requests
        /// </summary>
        private readonly HttpClient client;

        public Factory(HttpClient client)
        {
            this.client = client;
        }

        /// <summary>
        /// Intercepts requests at the latest possible stage
        /// (this function is injected into each PendingRequest instance)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private Response RequestInterceptor(Request request)
        {
            foreach (var callback in stubCallbacks)
            {
                var response = callback?.Invoke(request);

                if (response != null)
                    return response; // fake response
            }

            return null; // no faking occurs
        }
        
        #region "Faking methods"

        /// <summary>
        /// Intercept all requests to make them testable
        /// </summary>
        /// <returns></returns>
        public Factory Fake()
            => Fake("*");

        /// <summary>
        /// Intercept all requests going to a matching URL
        /// </summary>
        /// <param name="urlPattern">Wildcard pattern with asterisks</param>
        /// <returns></returns>
        public Factory Fake(string urlPattern)
            => Fake(urlPattern, Response.Create());

        /// <summary>
        /// Intercept all requests and respond with the given response
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public Factory Fake(Response response)
            => Fake("*", response);

        /// <summary>
        /// Intercept all requests going to a matching URL and respond
        /// with a given response
        /// </summary>
        /// <param name="urlPattern">Wildcard pattern with asterisks</param>
        /// <param name="response"></param>
        /// <returns></returns>
        public Factory Fake(string urlPattern, Response response)
            => Fake(urlPattern, request => response);

        /// <summary>
        /// Intercept all requests and respond with a sequence of responses
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public Factory Fake(ResponseSequence sequence)
            => Fake("*", sequence);
        
        /// <summary>
        /// Intercept all requests going to a certain URL and respond
        /// with a sequence of responses
        /// </summary>
        /// <param name="urlPattern">Wildcard pattern with asterisks</param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public Factory Fake(string urlPattern, ResponseSequence sequence)
            => Fake(urlPattern, request => sequence.Next());

        /// <summary>
        /// Intercept all requests going to a certain URL and give them
        /// to a callback that may fake their response or do nothing
        /// if it returns null.
        /// </summary>
        /// <param name="urlPattern">Wildcard pattern with asterisks</param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Factory Fake(string urlPattern, Func<Request, Response> callback)
            => RegisterStubCallbackForUrl(urlPattern, callback);

        /// <summary>
        /// Intercept all requests and give them to a callback that may fake
        /// their response or do nothing if it returns null.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Factory Fake(Func<Request, Response> callback)
            => RegisterStubCallback(callback);

        private Factory RegisterStubCallbackForUrl(
            string urlPattern,
            Func<Request, Response> callback
        )
        {
            // automatically add wildcard at the beginning
            // (so we can write "github.com/..." and not care about protocol)
            string fullPattern = "*" + urlPattern;
            
            RegisterStubCallback(
                request => Str.Is(request.Url, fullPattern)
                    ? callback?.Invoke(request)
                    : null
            );

            return this;
        }
        
        private Factory RegisterStubCallback(Func<Request, Response> callback)
        {
            stubCallbacks.Add(callback);
            IsRecording = true;
            
            return this;
        }
        
        #endregion
        
        #region "Recording"

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
        
        #endregion
        
        #region "Assertions"

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
        
        #endregion

        /// <summary>
        /// Creates a new instance of the pending request
        /// </summary>
        public PendingRequest PendingRequest()
        {
            return new PendingRequest(client, RequestInterceptor);
        }
    }
}