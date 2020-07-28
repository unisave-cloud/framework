using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Unisave.Exceptions;
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
        /// List of recorded request-response pairs
        /// </summary>
        private readonly List<Record> recorded = new List<Record>();
        
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
        /// <param name="next">Rest of the request processing</param>
        /// <returns></returns>
        private Response RequestInterceptor(
            Request request,
            Func<Response> next
        )
        {
            Response response = null;
            
            // stubbing
            foreach (var callback in stubCallbacks)
            {
                response = callback?.Invoke(request);

                if (response != null)
                    break;
            }

            try
            {
                // invocation
                if (response == null)
                    response = next?.Invoke();
            }
            finally
            {
                // recording
                if (IsRecording)
                    recorded.Add(new Record(request, response));
            }
            
            return response;
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

        /// <summary>
        /// Returns all recorded request-response pairs
        /// </summary>
        public List<Record> Recorded()
            => Recorded((request, response) => true);
        
        /// <summary>
        /// Returns all recorded request-response pairs
        /// that fulfill the condition
        /// </summary>
        public List<Record> Recorded(Func<Request, bool> condition)
            => Recorded(
                (request, response) => condition?.Invoke(request)
                    ?? throw new ArgumentNullException(nameof(condition))
                );
        
        /// <summary>
        /// Returns all recorded request-response pairs
        /// that fulfill the condition
        /// </summary>
        public List<Record> Recorded(Func<Request, Response, bool> condition)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
            
            return recorded
                .Where(record => condition.Invoke(
                    record.Request,
                    record.Response
                ))
                .ToList();
        }
        
        #endregion
        
        #region "Assertions"

        public void AssertSent(Func<Request, bool> condition)
            => AssertSent(
                (request, response) => condition?.Invoke(request)
                ?? throw new ArgumentNullException(nameof(condition))
            );
        
        public void AssertSent(Func<Request, Response, bool> condition)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
            
            var matching = Recorded(condition);
            
            if (matching.Count == 0)
                throw new UnisaveAssertionException(
                    "There were no requests, matching the specified condition."
                );
        }
        
        public void AssertNotSent(Func<Request, bool> condition)
            => AssertNotSent(
                (request, response) => condition?.Invoke(request)
                ?? throw new ArgumentNullException(nameof(condition))
            );
        
        public void AssertNotSent(Func<Request, Response, bool> condition)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
            
            var matching = Recorded(condition);
            
            if (matching.Count > 0)
                throw new UnisaveAssertionException(
                    "There were requests, matching the specified condition:\n" +
                    string.Join("\n", matching.Select(
                        r => r.Request.Original.Method + " " + r.Request.Url
                    ))
                );
        }

        public void AssertNothingSent()
        {
            if (recorded.Count > 0)
                throw new UnisaveAssertionException(
                    "No requests should have been sent but there were " +
                    "following requests:\n" +
                    string.Join("\n", recorded.Select(
                        r => r.Request.Original.Method + " " + r.Request.Url
                    ))
                );
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