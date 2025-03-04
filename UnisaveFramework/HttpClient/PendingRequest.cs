using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using LightJson;

namespace Unisave.HttpClient
{
    /// <summary>
    /// Fluently builds an HTTP request description and then provides
    /// a set of methods to execute the request.
    /// </summary>
    public class PendingRequest
    {
        /// <summary>
        /// Header to add to the request
        /// </summary>
        private Dictionary<string, string> headers;

        /// <summary>
        /// Authentication parameters
        /// When null: no authentication
        /// Else:
        /// ["_scheme"] = "Bearer" / "Digest" / "Basic" / ...
        /// ["username"], ["password"], ["token"], ...
        /// </summary>
        private Dictionary<string, string> auth;

        /// <summary>
        /// Content (payload) of the request, as is currently built up
        /// </summary>
        private HttpContent content;
        
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
        
        #region "Request construction"

        /// <summary>
        /// Sets additional request headers
        /// </summary>
        /// <param name="requestHeaders"></param>
        /// <returns></returns>
        public PendingRequest WithHeaders(Dictionary<string, string> requestHeaders)
        {
            headers = requestHeaders;
            return this;
        }

        /// <summary>
        /// Specifies the request body via the .NET HttpContent class.
        /// Null represent no body.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public PendingRequest WithBody(HttpContent body)
        {
            content = body;
            return this;
        }

        /// <summary>
        /// Specifies the request body as a form url encoded content
        /// </summary>
        /// <param name="form">Content</param>
        /// <returns></returns>
        public PendingRequest WithFormBody(Dictionary<string, string> form)
        {
            if (form == null)
                form = new Dictionary<string, string>();
            
            content = new FormUrlEncodedContent(form);
            
            return this;
        }

        /// <summary>
        /// Specifies the request body as a JSON object
        /// </summary>
        /// <param name="json">The JSON object</param>
        /// <returns></returns>
        public PendingRequest WithJsonBody(JsonObject json)
        {
            if (json == null)
                json = new JsonObject();

            content = new StringContent(
                json.ToString(),
                Encoding.UTF8,
                "application/json"
            );
            
            return this;
        }

        /// <summary>
        /// Attaches a JSON part to the multipart content
        /// </summary>
        /// <param name="name">Part name</param>
        /// <param name="jsonPart">Json content</param>
        /// <param name="fileName">Filename if the part is a file</param>
        /// <param name="contentHeaders">Additional part headers</param>
        /// <returns></returns>
        public PendingRequest Attach(
            string name,
            JsonObject jsonPart,
            string fileName = null,
            Dictionary<string, string> contentHeaders = null
        )
        {
            return Attach(
                name,
                new StringContent(
                    jsonPart.ToString(),
                    Encoding.UTF8,
                    "application/json"
                ),
                fileName,
                contentHeaders
            );
        }

        /// <summary>
        /// Attaches a part to the multipart content
        /// </summary>
        /// <param name="name">Part name</param>
        /// <param name="part">Part content</param>
        /// <param name="fileName">Filename if the part is a file</param>
        /// <param name="contentHeaders">Additional part headers</param>
        /// <returns></returns>
        public PendingRequest Attach(
            string name,
            HttpContent part,
            string fileName = null,
            Dictionary<string, string> contentHeaders = null
        )
        {
            var mpContent = content as MultipartFormDataContent;
            
            if (mpContent == null)
                content = mpContent = new MultipartFormDataContent();
            
            AddContentHeaders(part.Headers, contentHeaders);
            
            if (fileName == null)
                mpContent.Add(part, name);
            else
                mpContent.Add(part, name, fileName);
            
            return this;
        }
        
        private void AddContentHeaders(
            HttpContentHeaders dotNetHeaders,
            Dictionary<string, string> contentHeaders
        )
        {
            if (contentHeaders == null)
                return;
            
            foreach (var pair in contentHeaders)
            {
                if (!dotNetHeaders.TryAddWithoutValidation(pair.Key, pair.Value))
                {
                    throw new ArgumentException(
                        $"The header {pair.Key} cannot be specified via the " +
                        $"method {nameof(Attach)}. The header probably " +
                        $"refers to the whole request and so cannot be " +
                        $"attached to a content part."
                    );
                }
            }
        }

        /// <summary>
        /// Adds authentication data to the request
        /// (basic authentication from RFC 7617)
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public PendingRequest WithBasicAuth(string username, string password)
        {
            auth = new Dictionary<string, string> {
                ["_scheme"] = "Basic",
                ["username"] = username,
                ["password"] = password
            };
            
            return this;
        }
        
        /// <summary>
        /// Adds authentication data to the request
        /// (bearer authentication of OAuth 2.0 from RFC 6750)
        /// </summary>
        /// <param name="bearerToken"></param>
        /// <returns></returns>
        public PendingRequest WithToken(string bearerToken)
        {
            auth = new Dictionary<string, string> {
                ["_scheme"] = "Bearer",
                ["token"] = bearerToken
            };
            
            return this;
        }
        
        #endregion
        
        #region "Request sending"

        /// <summary>
        /// Sends a GET request
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="query">Query parameters</param>
        /// <returns></returns>
        public Response Get(string url, Dictionary<string, string> query = null)
        {
            return Send(HttpMethod.Get, url, query);
        }

        /// <summary>
        /// Sends a POST request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns></returns>
        public Response Post(string url, Dictionary<string, string> form)
        {
            WithFormBody(form);
            
            return Send(HttpMethod.Post, url);
        }
        
        /// <summary>
        /// Sends a POST request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns></returns>
        public Response Post(string url, JsonObject json)
        {
            WithJsonBody(json);
            
            return Send(HttpMethod.Post, url);
        }
        
        /// <summary>
        /// Sends a POST request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns></returns>
        public Response Post(string url)
        {
            return Send(HttpMethod.Post, url);
        }
        
        /// <summary>
        /// Sends a PUT request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns></returns>
        public Response Put(string url, Dictionary<string, string> form)
        {
            WithFormBody(form);
            
            return Send(HttpMethod.Put, url);
        }
        
        /// <summary>
        /// Sends a PUT request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns></returns>
        public Response Put(string url, JsonObject json)
        {
            WithJsonBody(json);
            
            return Send(HttpMethod.Put, url);
        }
        
        /// <summary>
        /// Sends a PUT request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns></returns>
        public Response Put(string url)
        {
            return Send(HttpMethod.Put, url);
        }
        
        /// <summary>
        /// Sends a PATCH request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns></returns>
        public Response Patch(string url, Dictionary<string, string> form)
        {
            WithFormBody(form);
            
            return Send(new HttpMethod("PATCH"), url);
        }
        
        /// <summary>
        /// Sends a PATCH request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns></returns>
        public Response Patch(string url, JsonObject json)
        {
            WithJsonBody(json);
            
            return Send(new HttpMethod("PATCH"), url);
        }
        
        /// <summary>
        /// Sends a PATCH request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns></returns>
        public Response Patch(string url)
        {
            return Send(new HttpMethod("PATCH"), url);
        }
        
        /// <summary>
        /// Sends a DELETE request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns></returns>
        public Response Delete(string url, Dictionary<string, string> form)
        {
            WithFormBody(form);
            
            return Send(HttpMethod.Delete, url);
        }
        
        /// <summary>
        /// Sends a DELETE request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns></returns>
        public Response Delete(string url, JsonObject json)
        {
            WithJsonBody(json);
            
            return Send(HttpMethod.Delete, url);
        }
        
        /// <summary>
        /// Sends a DELETE request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns></returns>
        public Response Delete(string url)
        {
            return Send(HttpMethod.Delete, url);
        }
        
        /// <summary>
        /// Sends an HTTP request with the method specified as a parameter
        /// </summary>
        /// <param name="method">HTTP method</param>
        /// <param name="url">Target URL</param>
        /// <param name="query">Query string to put into the URL</param>
        /// <returns></returns>
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

        private void AddRequestHeaders(HttpRequestMessage request)
        {
            if (headers == null)
                return;
            
            foreach (var pair in headers)
            {
                if (!request.Headers.TryAddWithoutValidation(pair.Key, pair.Value))
                {
                    throw new ArgumentException(
                        $"The header {pair.Key} cannot be specified via the " +
                        $"method {nameof(WithHeaders)}. The header probably " +
                        $"refers to the content and will be determined " +
                        $"automatically. In other cases use the " +
                        $"{nameof(WithBody)} method to specify content headers."
                    );
                }
            }
        }

        private void AddAuthorizationHeader(HttpRequestMessage request)
        {
            if (auth == null)
                return;

            string scheme = auth["_scheme"];
            string parameter;

            switch (scheme)
            {
                case "Basic":
                    parameter = Convert.ToBase64String(
                        Encoding.UTF8.GetBytes(
                            auth["username"] + ":" + auth["password"]
                        )
                    );
                    break;
                
                case "Bearer":
                    parameter = auth["token"];
                    break;
                
                default:
                    throw new InvalidOperationException(
                        "Unknown authorization scheme: " + scheme
                    );
            }
            
            request.Headers.Authorization = new AuthenticationHeaderValue(
                scheme, parameter
            );
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
        
        #endregion
    }
}