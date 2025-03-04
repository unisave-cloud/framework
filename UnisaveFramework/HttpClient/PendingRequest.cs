using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Unisave.HttpClient
{
    using InterceptorFunc = Func<Request, Func<Task<Response>>, Task<Response>>;
    
    /// <summary>
    /// Fluently builds an HTTP request description and then provides
    /// a set of methods to execute the request.
    /// </summary>
    public partial class PendingRequest
    {
        /// <summary>
        /// The underlying HttpClient instance that does the actual sending
        /// </summary>
        private readonly System.Net.Http.HttpClient client;

        /// <summary>
        /// Interceptor, that may fake responses
        /// </summary>
        private readonly InterceptorFunc interceptor;
        
        /// <summary>
        /// Whether the .Send() method (i.e. also the .Get() or .Post() methods)
        /// should finish after only reading the headers, or also the body.
        /// The default behaviour is reading the whole body, which lets the
        /// user access the response body synchronously with no synchronous
        /// waiting actually performed. However for streamed responses,
        /// this needs to be set to "only headers".
        /// </summary>
        private HttpCompletionOption httpCompletionOption
            = HttpCompletionOption.ResponseContentRead;

        public PendingRequest(
            System.Net.Http.HttpClient client,
            InterceptorFunc interceptor = null
        )
        {
            this.client = client;
            
            // default interceptor does nothing and calls the "next" callback
            this.interceptor = interceptor
                ?? ((request, next) => next.Invoke());
        }

        /// <summary>
        /// Only the response headers should be read, not the body. The body
        /// will be read later, once actually requested from the response
        /// object. After using this setup, you should access the response
        /// body via the asynchronous methods to avoid synchronous waiting.
        /// </summary>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public PendingRequest WithoutResponseBuffering()
        {
            httpCompletionOption = HttpCompletionOption.ResponseHeadersRead;
            return this;
        }

        /// <summary>
        /// Sends an HTTP request with the method specified as a parameter
        /// </summary>
        /// <param name="method">HTTP method</param>
        /// <param name="url">Target URL</param>
        /// <param name="query">Query string to put into the URL</param>
        /// <returns>The HTTP response object</returns>
        public Response Send(
            HttpMethod method,
            string url,
            Dictionary<string, string> query = null
        )
        {
            var task = SendAsync(method, url, query);
            
            // Schedule the task onto another thread and then synchronously
            // wait from this thread to prevent single-threaded deadlock:
            // https://github.com/unisave-cloud/worker/blob/master/docs/deadlocks.md
            var response = Task.Run(() => task).GetAwaiter().GetResult();

            return response;
        }
        
        /// <summary>
        /// Sends an HTTP request with the method specified as a parameter
        /// </summary>
        /// <param name="method">HTTP method</param>
        /// <param name="url">Target URL</param>
        /// <param name="query">Query string to put into the URL</param>
        /// <returns>The HTTP response object</returns>
        public async Task<Response> SendAsync(
            HttpMethod method,
            string url,
            Dictionary<string, string> query = null
        )
        {
            // prepare the .NET request
            var dotNetRequest = new HttpRequestMessage {
                Method = method,
                RequestUri = BuildUrl(url, query),
                Content = content
            };
            AddRequestHeaders(dotNetRequest);
            AddAuthorizationHeader(dotNetRequest);
            
            // prepare the request object
            var request = new Request(dotNetRequest);

            // define the "next" callback for the interceptor
            async Task<Response> Next() => await DoSendRequest(request);

            // pass the request through the interceptor and ultimately
            // into the "next" callback, thus sending the HTTP request
            return await interceptor.Invoke(request, Next);
        }

        /// <summary>
        /// Joins the URL string with the query parameters and validates
        /// the resulting URL string
        /// </summary>
        private static Uri BuildUrl(
            string url,
            Dictionary<string, string> query
        )
        {
            var uriBuilder = new UriBuilder(url);
            
            if (query != null)
            {
                uriBuilder.Query = string.Join("&",
                    query.Select(pair =>
                        WebUtility.UrlEncode(pair.Key) + "=" +
                        WebUtility.UrlEncode(pair.Value)
                    )
                );
            }

            return uriBuilder.Uri;
        }

        /// <summary>
        /// Performs the actual sending operation and returns the response
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task<Response> DoSendRequest(Request request)
        {
            using (CancellationTokenSource cts = CreateLinkedCts())
            {
                return new Response(
                    await client.SendAsync(
                        request.Original,
                        httpCompletionOption,
                        cts.Token
                    )
                );
            }
        }
    }
}