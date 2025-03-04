using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LightJson;

namespace Unisave.HttpClient
{
    /// <summary>
    /// Implements the IHttp public API. This interface is what the user should
    /// use to interact with the Unisave HTTP client component.
    /// </summary>
    public class Client : IHttp
    {
        private readonly Factory factory;

        public Client(Factory factory)
        {
            this.factory = factory;
        }

        #region "Request sending"

        /// <summary>
        /// Sends a GET request
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="query">Query parameters</param>
        /// <returns>The HTTP response object</returns>
        public Response Get(string url, Dictionary<string, string> query = null)
            => factory.PendingRequest().Get(url, query);

        /// <summary>
        /// Sends a POST request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
        public Response Post(string url, Dictionary<string, string> form)
            => factory.PendingRequest().Post(url, form);

        /// <summary>
        /// Sends a POST request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns>The HTTP response object</returns>
        public Response Post(string url, JsonObject json)
            => factory.PendingRequest().Post(url, json);

        /// <summary>
        /// Sends a POST request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns>The HTTP response object</returns>
        public Response Post(string url)
            => factory.PendingRequest().Post(url);
        
        /// <summary>
        /// Sends a PUT request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
        public Response Put(string url, Dictionary<string, string> form)
            => factory.PendingRequest().Put(url, form);
        
        /// <summary>
        /// Sends a PUT request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns>The HTTP response object</returns>
        public Response Put(string url, JsonObject json)
            => factory.PendingRequest().Put(url, json);
        
        /// <summary>
        /// Sends a PUT request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns>The HTTP response object</returns>
        public Response Put(string url)
            => factory.PendingRequest().Put(url);
        
        /// <summary>
        /// Sends a PATCH request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
        public Response Patch(string url, Dictionary<string, string> form)
            => factory.PendingRequest().Patch(url, form);
        
        /// <summary>
        /// Sends a PATCH request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns>The HTTP response object</returns>
        public Response Patch(string url, JsonObject json)
            => factory.PendingRequest().Patch(url, json);
        
        /// <summary>
        /// Sends a PATCH request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns>The HTTP response object</returns>
        public Response Patch(string url)
            => factory.PendingRequest().Patch(url);
        
        /// <summary>
        /// Sends a DELETE request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
        public Response Delete(string url, Dictionary<string, string> form)
            => factory.PendingRequest().Delete(url, form);
        
        /// <summary>
        /// Sends a DELETE request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns>The HTTP response object</returns>
        public Response Delete(string url, JsonObject json)
            => factory.PendingRequest().Delete(url, json);
        
        /// <summary>
        /// Sends a DELETE request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns>The HTTP response object</returns>
        public Response Delete(string url)
            => factory.PendingRequest().Delete(url);

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
        ) => factory.PendingRequest().Send(method, url, query);

        #endregion
        
        #region "Async request sending"

        /// <summary>
        /// Sends a GET request
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="query">Query parameters</param>
        /// <returns>The HTTP response object</returns>
        public Task<Response> GetAsync(
            string url,
            Dictionary<string, string> query = null
        ) => factory.PendingRequest().GetAsync(url, query);

        /// <summary>
        /// Sends a POST request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
        public Task<Response> PostAsync(
            string url,
            Dictionary<string, string> form
        ) => factory.PendingRequest().PostAsync(url, form);

        /// <summary>
        /// Sends a POST request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns>The HTTP response object</returns>
        public Task<Response> PostAsync(string url, JsonObject json)
            => factory.PendingRequest().PostAsync(url, json);

        /// <summary>
        /// Sends a POST request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns>The HTTP response object</returns>
        public Task<Response> PostAsync(string url)
            => factory.PendingRequest().PostAsync(url);
        
        /// <summary>
        /// Sends a PUT request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
        public Task<Response> PutAsync(
            string url,
            Dictionary<string, string> form
        ) => factory.PendingRequest().PutAsync(url, form);
        
        /// <summary>
        /// Sends a PUT request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns>The HTTP response object</returns>
        public Task<Response> PutAsync(string url, JsonObject json)
            => factory.PendingRequest().PutAsync(url, json);
        
        /// <summary>
        /// Sends a PUT request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns>The HTTP response object</returns>
        public Task<Response> PutAsync(string url)
            => factory.PendingRequest().PutAsync(url);
        
        /// <summary>
        /// Sends a PATCH request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
        public Task<Response> PatchAsync(
            string url,
            Dictionary<string, string> form
        ) => factory.PendingRequest().PatchAsync(url, form);
        
        /// <summary>
        /// Sends a PATCH request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns>The HTTP response object</returns>
        public Task<Response> PatchAsync(string url, JsonObject json)
            => factory.PendingRequest().PatchAsync(url, json);
        
        /// <summary>
        /// Sends a PATCH request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns>The HTTP response object</returns>
        public Task<Response> PatchAsync(string url)
            => factory.PendingRequest().PatchAsync(url);
        
        /// <summary>
        /// Sends a DELETE request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
        public Task<Response> DeleteAsync(
            string url,
            Dictionary<string, string> form
        ) => factory.PendingRequest().DeleteAsync(url, form);
        
        /// <summary>
        /// Sends a DELETE request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns>The HTTP response object</returns>
        public Task<Response> DeleteAsync(string url, JsonObject json)
            => factory.PendingRequest().DeleteAsync(url, json);
        
        /// <summary>
        /// Sends a DELETE request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns>The HTTP response object</returns>
        public Task<Response> DeleteAsync(string url)
            => factory.PendingRequest().DeleteAsync(url);

        /// <summary>
        /// Sends an HTTP request with the method specified as a parameter
        /// </summary>
        /// <param name="method">HTTP method</param>
        /// <param name="url">Target URL</param>
        /// <param name="query">Query string to put into the URL</param>
        /// <returns>The HTTP response object</returns>
        public Task<Response> SendAsync(
            HttpMethod method,
            string url,
            Dictionary<string, string> query = null
        ) => factory.PendingRequest().SendAsync(method, url, query);

        #endregion
        
        #region "Request construction"

        /// <summary>
        /// Creates a new instance of pending request
        /// </summary>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public PendingRequest Request() => factory.PendingRequest();

        /// <summary>
        /// Sets additional request headers. When invoked multiple times,
        /// the previous values are forgotten.
        /// </summary>
        /// <param name="requestHeaders"></param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public PendingRequest WithHeaders(
            Dictionary<string, string> requestHeaders
        ) => factory.PendingRequest().WithHeaders(requestHeaders);

        /// <summary>
        /// Specifies the request body via the .NET HttpContent class.
        /// Null represent no body.
        /// </summary>
        /// <param name="body"></param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public PendingRequest WithBody(HttpContent body)
            => factory.PendingRequest().WithBody(body);

        /// <summary>
        /// Specifies the request body as a form url encoded content
        /// </summary>
        /// <param name="form">Content</param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public PendingRequest WithFormBody(Dictionary<string, string> form)
            => factory.PendingRequest().WithFormBody(form);

        /// <summary>
        /// Specifies the request body as a JSON object
        /// </summary>
        /// <param name="json">The JSON object</param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public PendingRequest WithJsonBody(JsonObject json)
            => factory.PendingRequest().WithJsonBody(json);

        /// <summary>
        /// Attaches a JSON part to the multipart content
        /// </summary>
        /// <param name="name">Part name</param>
        /// <param name="jsonPart">Json content</param>
        /// <param name="fileName">Filename if the part is a file</param>
        /// <param name="contentHeaders">Additional part headers</param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public PendingRequest Attach(
            string name,
            JsonObject jsonPart,
            string fileName = null,
            Dictionary<string, string> contentHeaders = null
        ) => factory.PendingRequest()
            .Attach(name, jsonPart, fileName, contentHeaders);

        /// <summary>
        /// Attaches a part to the multipart content
        /// </summary>
        /// <param name="name">Part name</param>
        /// <param name="part">Part content</param>
        /// <param name="fileName">Filename if the part is a file</param>
        /// <param name="contentHeaders">Additional part headers</param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public PendingRequest Attach(
            string name,
            HttpContent part,
            string fileName = null,
            Dictionary<string, string> contentHeaders = null
        ) => factory.PendingRequest()
            .Attach(name, part, fileName, contentHeaders);

        /// <summary>
        /// Adds authentication data to the request
        /// (basic authentication from RFC 7617)
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public PendingRequest WithBasicAuth(string username, string password)
            => factory.PendingRequest().WithBasicAuth(username, password);

        /// <summary>
        /// Adds authentication data to the request
        /// (bearer authentication of OAuth 2.0 from RFC 6750)
        /// </summary>
        /// <param name="bearerToken"></param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public PendingRequest WithToken(string bearerToken)
            => factory.PendingRequest().WithToken(bearerToken);
        
        /// <summary>
        /// Sets the cancellation token that cancels this request
        /// </summary>
        /// <param name="token"></param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public PendingRequest WithCancellation(CancellationToken token)
            => factory.PendingRequest().WithCancellation(token);

        /// <summary>
        /// Sets the maximum number of seconds to wait for a response.
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public PendingRequest WithTimeout(double seconds)
            => factory.PendingRequest().WithTimeout(seconds);

        /// <summary>
        /// Sets the maximum number of time to wait for a response.
        /// </summary>
        /// <param name="time"></param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public PendingRequest WithTimeout(TimeSpan time)
            => factory.PendingRequest().WithTimeout(time);

        /// <summary>
        /// Only the response headers should be read, not the body. The body
        /// will be read later, once actually requested from the response
        /// object. After using this setup, you should access the response
        /// body via the asynchronous methods to avoid synchronous waiting.
        /// </summary>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public PendingRequest WithoutResponseBuffering()
            => factory.PendingRequest().WithoutResponseBuffering();

        #endregion
        
        #region "Faking"

        /// <summary>
        /// Intercept all requests to make them testable
        /// </summary>
        /// <returns>Itself for chaining</returns>
        public IHttp Fake()
        {
            factory.Fake();
            return this;
        }

        /// <summary>
        /// Intercept all requests going to a matching URL
        /// </summary>
        /// <param name="urlPattern">Wildcard pattern with asterisks</param>
        /// <returns>Itself for chaining</returns>
        public IHttp Fake(string urlPattern)
        {
            factory.Fake(urlPattern);
            return this;
        }

        /// <summary>
        /// Intercept all requests and respond with the given response
        /// </summary>
        /// <param name="response"></param>
        /// <returns>Itself for chaining</returns>
        public IHttp Fake(Response response)
        {
            factory.Fake(response);
            return this;
        }

        /// <summary>
        /// Intercept all requests going to a matching URL and respond
        /// with a given response
        /// </summary>
        /// <param name="urlPattern">Wildcard pattern with asterisks</param>
        /// <param name="response"></param>
        /// <returns>Itself for chaining</returns>
        public IHttp Fake(string urlPattern, Response response)
        {
            factory.Fake(urlPattern, response);
            return this;
        }

        /// <summary>
        /// Intercept all requests and respond with a sequence of responses
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns>Itself for chaining</returns>
        public IHttp Fake(ResponseSequence sequence)
        {
            factory.Fake(sequence);
            return this;
        }

        /// <summary>
        /// Intercept all requests going to a certain URL and respond
        /// with a sequence of responses
        /// </summary>
        /// <param name="urlPattern">Wildcard pattern with asterisks</param>
        /// <param name="sequence"></param>
        /// <returns>Itself for chaining</returns>
        public IHttp Fake(string urlPattern, ResponseSequence sequence)
        {
            factory.Fake(urlPattern, sequence);
            return this;
        }

        /// <summary>
        /// Intercept all requests going to a certain URL and give them
        /// to a callback that may fake their response or do nothing
        /// if it returns null.
        /// </summary>
        /// <param name="urlPattern">Wildcard pattern with asterisks</param>
        /// <param name="callback"></param>
        /// <returns>Itself for chaining</returns>
        public IHttp Fake(string urlPattern, Func<Request, Response> callback)
        {
            factory.Fake(urlPattern, callback);
            return this;
        }

        /// <summary>
        /// Intercept all requests and give them to a callback that may fake
        /// their response or do nothing if it returns null.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns>Itself for chaining</returns>
        public IHttp Fake(Func<Request, Response> callback)
        {
            factory.Fake(callback);
            return this;
        }

        #endregion
        
        #region "Recording"

        /// <summary>
        /// Returns all recorded request-response pairs
        /// </summary>
        public List<RequestResponsePair> Recorded() => factory.Recorded();

        /// <summary>
        /// Returns all recorded request-response pairs
        /// that fulfill the condition
        /// </summary>
        public List<RequestResponsePair> Recorded(Func<Request, bool> condition)
            => factory.Recorded(condition);
        
        /// <summary>
        /// Returns all recorded request-response pairs
        /// that fulfill the condition
        /// </summary>
        public List<RequestResponsePair> Recorded(
            Func<Request, Response, bool> condition
        ) => factory.Recorded(condition);
        
        #endregion
        
        #region "Assertions"

        /// <summary>
        /// Throws a UnisaveAssertionException if there is no recorded request
        /// that matches the provided condition
        /// </summary>
        public void AssertSent(Func<Request, bool> condition)
            => factory.AssertSent(condition);

        /// <summary>
        /// Throws a UnisaveAssertionException if there is no recorded request
        /// that matches the provided condition
        /// </summary>
        public void AssertSent(Func<Request, Response, bool> condition)
            => factory.AssertSent(condition);

        /// <summary>
        /// Throws a UnisaveAssertionException if there is a recorded request
        /// that matches the provided condition
        /// </summary>
        public void AssertNotSent(Func<Request, bool> condition)
            => factory.AssertNotSent(condition);

        /// <summary>
        /// Throws a UnisaveAssertionException if there is a recorded request
        /// that matches the provided condition
        /// </summary>
        public void AssertNotSent(Func<Request, Response, bool> condition)
            => factory.AssertNotSent(condition);

        /// <summary>
        /// Throws a UnisaveAssertionException if there is a recorded request
        /// </summary>
        public void AssertNothingSent() => factory.AssertNothingSent();

        #endregion
        
        #region "Stubbing"

        /// <summary>
        /// Creates a stub JSON response, used for testing
        /// </summary>
        /// <param name="json">Response JSON body</param>
        /// <param name="status">HTTP status code</param>
        /// <param name="headers">Additional response headers</param>
        /// <returns>Stub HTTP response object</returns>
        public Response Response(
            JsonObject json,
            int status = 200,
            Dictionary<string, string> headers = null
        ) => HttpClient.Response.Create(json, status, headers);

        /// <summary>
        /// Creates a stub string response, used for testing
        /// </summary>
        /// <param name="body">Response body</param>
        /// <param name="contentType">Text MIME type</param>
        /// <param name="status">HTTP response status code</param>
        /// <param name="headers">Additional response headers</param>
        /// <returns>Stub HTTP response object</returns>
        public Response Response(
            string body,
            string contentType = "text/plain",
            int status = 200,
            Dictionary<string, string> headers = null
        ) => HttpClient.Response.Create(body, contentType, status, headers);

        /// <summary>
        /// Creates a stub response, used for testing
        /// </summary>
        /// <param name="body">Response body</param>
        /// <param name="status">HTTP status code</param>
        /// <param name="headers">Response headers</param>
        /// <returns>Stub HTTP response object</returns>
        /// <exception cref="ArgumentException">Invalid headers</exception>
        public Response Response(
            HttpContent body = null,
            int status = 200,
            Dictionary<string, string> headers = null
        ) => HttpClient.Response.Create(body, status, headers);

        /// <summary>
        /// Creates a stub response sequence, used for testing
        /// </summary>
        /// <returns>
        /// Fluent builder for a sequence of stub HTTP responses
        /// </returns>
        public ResponseSequence Sequence() => new ResponseSequence();

        #endregion
    }
}