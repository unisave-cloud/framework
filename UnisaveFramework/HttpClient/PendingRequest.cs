using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Unisave.HttpClient
{
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
        private Func<Request, Func<Response>, Response> interceptor;

        public PendingRequest(
            System.Net.Http.HttpClient client,
            Func<Request, Func<Response>, Response> interceptor
        ) : this(client)
        {
            this.interceptor = interceptor;
        }
        
        public PendingRequest(System.Net.Http.HttpClient client)
        {
            this.client = client;
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
            // prepare .NET request
            var dotNetRequest = new HttpRequestMessage {
                Method = method,
                RequestUri = BuildUrl(url, query),
                Content = content
            };
            AddRequestHeaders(dotNetRequest);
            AddAuthorizationHeader(dotNetRequest);
            
            // prepare request
            var request = new Request(dotNetRequest);

            // perform interception
            if (interceptor == null)
                interceptor = (r, n) => n.Invoke();
            
            return interceptor.Invoke(request, () => {
                
                // perform sending
                return PerformSending(request);
            });
        }

        private Uri BuildUrl(string url, Dictionary<string, string> query)
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
        private Response PerformSending(Request request)
        {
            var task = client.SendAsync(request.Original);
            
            var dotNetResponse = Task.Run(() => task).GetAwaiter().GetResult();

            return new Response(dotNetResponse);
        }
    }
}