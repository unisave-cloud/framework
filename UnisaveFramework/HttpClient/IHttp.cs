using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using LightJson;

namespace Unisave.HttpClient
{
    /// <summary>
    /// Defines the public API of the HTTP Client component. It corresponds to
    /// the HTTP facade and should be used for dependency injection
    /// from the service container.
    /// </summary>
    public interface IHttp
    {
        #region "Request sending"

        /// <summary>
        /// Sends a GET request
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="query">Query parameters</param>
        /// <returns>The HTTP response object</returns>
        Response Get(string url, Dictionary<string, string> query = null);

        /// <summary>
        /// Sends a POST request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
        Response Post(string url, Dictionary<string, string> form);

        /// <summary>
        /// Sends a POST request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns>The HTTP response object</returns>
        Response Post(string url, JsonObject json);

        /// <summary>
        /// Sends a POST request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns>The HTTP response object</returns>
        Response Post(string url);
        
        /// <summary>
        /// Sends a PUT request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
        Response Put(string url, Dictionary<string, string> form);
        
        /// <summary>
        /// Sends a PUT request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns>The HTTP response object</returns>
        Response Put(string url, JsonObject json);
        
        /// <summary>
        /// Sends a PUT request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns>The HTTP response object</returns>
        Response Put(string url);
        
        /// <summary>
        /// Sends a PATCH request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
        Response Patch(string url, Dictionary<string, string> form);
        
        /// <summary>
        /// Sends a PATCH request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns>The HTTP response object</returns>
        Response Patch(string url, JsonObject json);
        
        /// <summary>
        /// Sends a PATCH request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns>The HTTP response object</returns>
        Response Patch(string url);
        
        /// <summary>
        /// Sends a DELETE request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
        Response Delete(string url, Dictionary<string, string> form);
        
        /// <summary>
        /// Sends a DELETE request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns>The HTTP response object</returns>
        Response Delete(string url, JsonObject json);
        
        /// <summary>
        /// Sends a DELETE request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns>The HTTP response object</returns>
        Response Delete(string url);

        /// <summary>
        /// Sends an HTTP request with the method specified as a parameter
        /// </summary>
        /// <param name="method">HTTP method</param>
        /// <param name="url">Target URL</param>
        /// <param name="query">Query string to put into the URL</param>
        /// <returns>The HTTP response object</returns>
        Response Send(
            HttpMethod method,
            string url,
            Dictionary<string, string> query = null
        );

        #endregion

        #region "Async request sending"

        /// <summary>
        /// Sends a GET request
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="query">Query parameters</param>
        /// <returns>The HTTP response object</returns>
        Task<Response> GetAsync(
            string url,
            Dictionary<string, string> query = null
        );

        /// <summary>
        /// Sends a POST request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
        Task<Response> PostAsync(string url, Dictionary<string, string> form);

        /// <summary>
        /// Sends a POST request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns>The HTTP response object</returns>
        Task<Response> PostAsync(string url, JsonObject json);

        /// <summary>
        /// Sends a POST request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns>The HTTP response object</returns>
        Task<Response> PostAsync(string url);
        
        /// <summary>
        /// Sends a PUT request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
        Task<Response> PutAsync(string url, Dictionary<string, string> form);
        
        /// <summary>
        /// Sends a PUT request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns>The HTTP response object</returns>
        Task<Response> PutAsync(string url, JsonObject json);
        
        /// <summary>
        /// Sends a PUT request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns>The HTTP response object</returns>
        Task<Response> PutAsync(string url);
        
        /// <summary>
        /// Sends a PATCH request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
        Task<Response> PatchAsync(string url, Dictionary<string, string> form);
        
        /// <summary>
        /// Sends a PATCH request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns>The HTTP response object</returns>
        Task<Response> PatchAsync(string url, JsonObject json);
        
        /// <summary>
        /// Sends a PATCH request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns>The HTTP response object</returns>
        Task<Response> PatchAsync(string url);
        
        /// <summary>
        /// Sends a DELETE request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
        Task<Response> DeleteAsync(string url, Dictionary<string, string> form);
        
        /// <summary>
        /// Sends a DELETE request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns>The HTTP response object</returns>
        Task<Response> DeleteAsync(string url, JsonObject json);
        
        /// <summary>
        /// Sends a DELETE request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns>The HTTP response object</returns>
        Task<Response> DeleteAsync(string url);

        /// <summary>
        /// Sends an HTTP request with the method specified as a parameter
        /// </summary>
        /// <param name="method">HTTP method</param>
        /// <param name="url">Target URL</param>
        /// <param name="query">Query string to put into the URL</param>
        /// <returns>The HTTP response object</returns>
        Task<Response> SendAsync(
            HttpMethod method,
            string url,
            Dictionary<string, string> query = null
        );

        #endregion
        
        #region "Request construction"

        /// <summary>
        /// Creates a new instance of pending request
        /// </summary>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        PendingRequest Request();

        /// <summary>
        /// Sets additional request headers
        /// </summary>
        /// <param name="requestHeaders"></param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        PendingRequest WithHeaders(Dictionary<string, string> requestHeaders);

        /// <summary>
        /// Specifies the request body via the .NET HttpContent class.
        /// Null represent no body.
        /// </summary>
        /// <param name="body"></param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        PendingRequest WithBody(HttpContent body);

        /// <summary>
        /// Specifies the request body as a form url encoded content
        /// </summary>
        /// <param name="form">Content</param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        PendingRequest WithFormBody(Dictionary<string, string> form);

        /// <summary>
        /// Specifies the request body as a JSON object
        /// </summary>
        /// <param name="json">The JSON object</param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        PendingRequest WithJsonBody(JsonObject json);

        /// <summary>
        /// Attaches a JSON part to the multipart content
        /// </summary>
        /// <param name="name">Part name</param>
        /// <param name="jsonPart">Json content</param>
        /// <param name="fileName">Filename if the part is a file</param>
        /// <param name="contentHeaders">Additional part headers</param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        PendingRequest Attach(
            string name,
            JsonObject jsonPart,
            string fileName = null,
            Dictionary<string, string> contentHeaders = null
        );

        /// <summary>
        /// Attaches a part to the multipart content
        /// </summary>
        /// <param name="name">Part name</param>
        /// <param name="part">Part content</param>
        /// <param name="fileName">Filename if the part is a file</param>
        /// <param name="contentHeaders">Additional part headers</param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        PendingRequest Attach(
            string name,
            HttpContent part,
            string fileName = null,
            Dictionary<string, string> contentHeaders = null
        );

        /// <summary>
        /// Adds authentication data to the request
        /// (basic authentication from RFC 7617)
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        PendingRequest WithBasicAuth(string username, string password);

        /// <summary>
        /// Adds authentication data to the request
        /// (bearer authentication of OAuth 2.0 from RFC 6750)
        /// </summary>
        /// <param name="bearerToken"></param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        PendingRequest WithToken(string bearerToken);

        #endregion
        
        #region "Faking"
        
        /// <summary>
        /// Intercept all requests to make them testable
        /// </summary>
        /// <returns>Itself for chaining</returns>
        IHttp Fake();

        /// <summary>
        /// Intercept all requests going to a matching URL
        /// </summary>
        /// <param name="urlPattern">Wildcard pattern with asterisks</param>
        /// <returns>Itself for chaining</returns>
        IHttp Fake(string urlPattern);

        /// <summary>
        /// Intercept all requests and respond with the given response
        /// </summary>
        /// <param name="response"></param>
        /// <returns>Itself for chaining</returns>
        IHttp Fake(Response response);

        /// <summary>
        /// Intercept all requests going to a matching URL and respond
        /// with a given response
        /// </summary>
        /// <param name="urlPattern">Wildcard pattern with asterisks</param>
        /// <param name="response"></param>
        /// <returns>Itself for chaining</returns>
        IHttp Fake(string urlPattern, Response response);

        /// <summary>
        /// Intercept all requests and respond with a sequence of responses
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns>Itself for chaining</returns>
        IHttp Fake(ResponseSequence sequence);
        
        /// <summary>
        /// Intercept all requests going to a certain URL and respond
        /// with a sequence of responses
        /// </summary>
        /// <param name="urlPattern">Wildcard pattern with asterisks</param>
        /// <param name="sequence"></param>
        /// <returns>Itself for chaining</returns>
        IHttp Fake(string urlPattern, ResponseSequence sequence);

        /// <summary>
        /// Intercept all requests going to a certain URL and give them
        /// to a callback that may fake their response or do nothing
        /// if it returns null.
        /// </summary>
        /// <param name="urlPattern">Wildcard pattern with asterisks</param>
        /// <param name="callback"></param>
        /// <returns>Itself for chaining</returns>
        IHttp Fake(string urlPattern, Func<Request, Response> callback);

        /// <summary>
        /// Intercept all requests and give them to a callback that may fake
        /// their response or do nothing if it returns null.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns>Itself for chaining</returns>
        IHttp Fake(Func<Request, Response> callback);

        #endregion
        
        #region "Recording"

        /// <summary>
        /// Returns all recorded request-response pairs
        /// </summary>
        List<RequestResponsePair> Recorded();

        /// <summary>
        /// Returns all recorded request-response pairs
        /// that fulfill the condition
        /// </summary>
        List<RequestResponsePair> Recorded(Func<Request, bool> condition);
        
        /// <summary>
        /// Returns all recorded request-response pairs
        /// that fulfill the condition
        /// </summary>
        List<RequestResponsePair> Recorded(
            Func<Request, Response, bool> condition
        );
        
        #endregion
        
        #region "Assertions"

        /// <summary>
        /// Throws a UnisaveAssertionException if there is no recorded request
        /// that matches the provided condition
        /// </summary>
        void AssertSent(Func<Request, bool> condition);

        /// <summary>
        /// Throws a UnisaveAssertionException if there is no recorded request
        /// that matches the provided condition
        /// </summary>
        void AssertSent(Func<Request, Response, bool> condition);

        /// <summary>
        /// Throws a UnisaveAssertionException if there is a recorded request
        /// that matches the provided condition
        /// </summary>
        void AssertNotSent(Func<Request, bool> condition);

        /// <summary>
        /// Throws a UnisaveAssertionException if there is a recorded request
        /// that matches the provided condition
        /// </summary>
        void AssertNotSent(Func<Request, Response, bool> condition);

        /// <summary>
        /// Throws a UnisaveAssertionException if there is a recorded request
        /// </summary>
        void AssertNothingSent();

        #endregion
        
        #region "Stubbing"

        /// <summary>
        /// Creates a stub JSON response, used for testing
        /// </summary>
        /// <param name="json">Response JSON body</param>
        /// <param name="status">HTTP status code</param>
        /// <param name="headers">Additional response headers</param>
        /// <returns>Stub HTTP response object</returns>
        Response Response(
            JsonObject json,
            int status = 200,
            Dictionary<string, string> headers = null
        );

        /// <summary>
        /// Creates a stub string response, used for testing
        /// </summary>
        /// <param name="body">Response body</param>
        /// <param name="contentType">Text MIME type</param>
        /// <param name="status">HTTP response status code</param>
        /// <param name="headers">Additional response headers</param>
        /// <returns>Stub HTTP response object</returns>
        Response Response(
            string body,
            string contentType = "text/plain",
            int status = 200,
            Dictionary<string, string> headers = null
        );

        /// <summary>
        /// Creates a stub response, used for testing
        /// </summary>
        /// <param name="body">Response body</param>
        /// <param name="status">HTTP status code</param>
        /// <param name="headers">Response headers</param>
        /// <returns>Stub HTTP response object</returns>
        /// <exception cref="ArgumentException">Invalid headers</exception>
        Response Response(
            HttpContent body = null,
            int status = 200,
            Dictionary<string, string> headers = null
        );

        /// <summary>
        /// Creates a stub response sequence, used for testing
        /// </summary>
        /// <returns>
        /// Fluent builder for a sequence of stub HTTP responses
        /// </returns>
        ResponseSequence Sequence();

        #endregion
    }
}