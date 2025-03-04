using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LightJson;
using Unisave.HttpClient;

namespace Unisave.Facades
{
    /// <summary>
    /// Facade for making HTTP requests
    /// </summary>
    public static class Http
    {
        private static IHttp GetHttp()
        {
            if (!Facade.CanUse)
                throw new InvalidOperationException(
                    "You cannot use the Http facade on the client side."
                );
            
            return Facade.Services.Resolve<IHttp>();
        }
        
        #region "Request sending"

        /// <summary>
        /// Sends a GET request
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="query">Query parameters</param>
        /// <returns>The HTTP response object</returns>
        public static Response Get(
            string url,
            Dictionary<string, string> query = null
        ) => GetHttp().Get(url, query);

        /// <summary>
        /// Sends a POST request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
        public static Response Post(string url, Dictionary<string, string> form)
            => GetHttp().Post(url, form);
        
        /// <summary>
        /// Sends a POST request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns>The HTTP response object</returns>
        public static Response Post(string url, JsonObject json)
            => GetHttp().Post(url, json);
        
        /// <summary>
        /// Sends a POST request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns>The HTTP response object</returns>
        public static Response Post(string url)
            => GetHttp().Post(url);
        
        /// <summary>
        /// Sends a PUT request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
        public static Response Put(string url, Dictionary<string, string> form)
            => GetHttp().Put(url, form);
        
        /// <summary>
        /// Sends a PUT request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns>The HTTP response object</returns>
        public static Response Put(string url, JsonObject json)
            => GetHttp().Put(url, json);
        
        /// <summary>
        /// Sends a PUT request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns>The HTTP response object</returns>
        public static Response Put(string url)
            => GetHttp().Put(url);
        
        /// <summary>
        /// Sends a PATCH request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
        public static Response Patch(
            string url,
            Dictionary<string, string> form
        ) => GetHttp().Patch(url, form);
        
        /// <summary>
        /// Sends a PATCH request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns>The HTTP response object</returns>
        public static Response Patch(string url, JsonObject json)
            => GetHttp().Patch(url, json);
        
        /// <summary>
        /// Sends a PATCH request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns>The HTTP response object</returns>
        public static Response Patch(string url)
            => GetHttp().Patch(url);
        
        /// <summary>
        /// Sends a DELETE request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
        public static Response Delete(
            string url,
            Dictionary<string, string> form
        ) => GetHttp().Delete(url, form);
        
        /// <summary>
        /// Sends a DELETE request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns>The HTTP response object</returns>
        public static Response Delete(string url, JsonObject json)
            => GetHttp().Delete(url, json);
        
        /// <summary>
        /// Sends a DELETE request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns>The HTTP response object</returns>
        public static Response Delete(string url)
            => GetHttp().Delete(url);
        
        /// <summary>
        /// Sends an HTTP request with the method specified as a parameter
        /// </summary>
        /// <param name="method">HTTP method</param>
        /// <param name="url">Target URL</param>
        /// <param name="query">Query string to put into the URL</param>
        /// <returns>The HTTP response object</returns>
        public static Response Send(
            HttpMethod method,
            string url,
            Dictionary<string, string> query = null
        ) => GetHttp().Send(method, url, query);
        
        #endregion
        
        #region "Async request sending"

        /// <summary>
        /// Sends a GET request
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="query">Query parameters</param>
        /// <returns>The HTTP response object</returns>
        public static Task<Response> GetAsync(
            string url,
            Dictionary<string, string> query = null
        ) => GetHttp().GetAsync(url, query);

        /// <summary>
        /// Sends a POST request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
        public static Task<Response> PostAsync(
            string url,
            Dictionary<string, string> form
        ) => GetHttp().PostAsync(url, form);

        /// <summary>
        /// Sends a POST request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns>The HTTP response object</returns>
        public static Task<Response> PostAsync(string url, JsonObject json)
            => GetHttp().PostAsync(url, json);

        /// <summary>
        /// Sends a POST request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns>The HTTP response object</returns>
        public static Task<Response> PostAsync(string url)
            => GetHttp().PostAsync(url);
        
        /// <summary>
        /// Sends a PUT request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
        public static Task<Response> PutAsync(
            string url,
            Dictionary<string, string> form
        ) => GetHttp().PutAsync(url, form);
        
        /// <summary>
        /// Sends a PUT request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns>The HTTP response object</returns>
        public static Task<Response> PutAsync(string url, JsonObject json)
            => GetHttp().PutAsync(url, json);
        
        /// <summary>
        /// Sends a PUT request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns>The HTTP response object</returns>
        public static Task<Response> PutAsync(string url)
            => GetHttp().PutAsync(url);
        
        /// <summary>
        /// Sends a PATCH request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
        public static  Task<Response> PatchAsync(
            string url,
            Dictionary<string, string> form
        ) => GetHttp().PatchAsync(url, form);
        
        /// <summary>
        /// Sends a PATCH request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns>The HTTP response object</returns>
        public static Task<Response> PatchAsync(string url, JsonObject json)
            => GetHttp().PatchAsync(url, json);
        
        /// <summary>
        /// Sends a PATCH request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns>The HTTP response object</returns>
        public static Task<Response> PatchAsync(string url)
            => GetHttp().PatchAsync(url);
        
        /// <summary>
        /// Sends a DELETE request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
        public static Task<Response> DeleteAsync(
            string url,
            Dictionary<string, string> form
        ) => GetHttp().DeleteAsync(url, form);
        
        /// <summary>
        /// Sends a DELETE request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns>The HTTP response object</returns>
        public static Task<Response> DeleteAsync(string url, JsonObject json)
            => GetHttp().DeleteAsync(url, json);
        
        /// <summary>
        /// Sends a DELETE request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns>The HTTP response object</returns>
        public static Task<Response> DeleteAsync(string url)
            => GetHttp().DeleteAsync(url);

        /// <summary>
        /// Sends an HTTP request with the method specified as a parameter
        /// </summary>
        /// <param name="method">HTTP method</param>
        /// <param name="url">Target URL</param>
        /// <param name="query">Query string to put into the URL</param>
        /// <returns>The HTTP response object</returns>
        public static Task<Response> SendAsync(
            HttpMethod method,
            string url,
            Dictionary<string, string> query = null
        ) => GetHttp().SendAsync(method, url, query);

        #endregion
        
        #region "Request construction"
        
        /// <summary>
        /// Creates a new instance of pending request
        /// </summary>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public static PendingRequest Request() => GetHttp().Request();

        /// <summary>
        /// Sets additional request headers
        /// </summary>
        /// <param name="requestHeaders"></param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public static PendingRequest WithHeaders(
            Dictionary<string, string> requestHeaders
        ) => GetHttp().WithHeaders(requestHeaders);

        /// <summary>
        /// Specifies the request body via the .NET HttpContent class.
        /// Null represent no body.
        /// </summary>
        /// <param name="body"></param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public static PendingRequest WithBody(HttpContent body)
            => GetHttp().WithBody(body);

        /// <summary>
        /// Specifies the request body as a form url encoded content
        /// </summary>
        /// <param name="form">Content</param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public static PendingRequest WithFormBody(
            Dictionary<string, string> form
        ) => GetHttp().WithFormBody(form);

        /// <summary>
        /// Specifies the request body as a JSON object
        /// </summary>
        /// <param name="json">The JSON object</param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public static PendingRequest WithJsonBody(JsonObject json)
            => GetHttp().WithJsonBody(json);

        /// <summary>
        /// Attaches a JSON part to the multipart content
        /// </summary>
        /// <param name="name">Part name</param>
        /// <param name="jsonPart">Json content</param>
        /// <param name="fileName">Filename if the part is a file</param>
        /// <param name="contentHeaders">Additional part headers</param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public static PendingRequest Attach(
            string name,
            JsonObject jsonPart,
            string fileName = null,
            Dictionary<string, string> contentHeaders = null
        ) => GetHttp().Attach(name, jsonPart, fileName, contentHeaders);

        /// <summary>
        /// Attaches a part to the multipart content
        /// </summary>
        /// <param name="name">Part name</param>
        /// <param name="part">Part content</param>
        /// <param name="fileName">Filename if the part is a file</param>
        /// <param name="contentHeaders">Additional part headers</param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public static PendingRequest Attach(
            string name,
            HttpContent part,
            string fileName = null,
            Dictionary<string, string> contentHeaders = null
        ) => GetHttp().Attach(name, part, fileName, contentHeaders);

        /// <summary>
        /// Adds authentication data to the request
        /// (basic authentication from RFC 7617)
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public static PendingRequest WithBasicAuth(
            string username,
            string password
        ) => GetHttp().WithBasicAuth(username, password);

        /// <summary>
        /// Adds authentication data to the request
        /// (bearer authentication of OAuth 2.0 from RFC 6750)
        /// </summary>
        /// <param name="bearerToken"></param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public static PendingRequest WithToken(string bearerToken)
            => GetHttp().WithToken(bearerToken);
        
        /// <summary>
        /// Sets the cancellation token that cancels this request
        /// </summary>
        /// <param name="token"></param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public static PendingRequest WithCancellation(CancellationToken token)
            => GetHttp().WithCancellation(token);

        /// <summary>
        /// Sets the maximum number of seconds to wait for a response.
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public static PendingRequest WithTimeout(double seconds)
            => GetHttp().WithTimeout(seconds);

        /// <summary>
        /// Sets the maximum number of time to wait for a response.
        /// </summary>
        /// <param name="time"></param>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public static PendingRequest WithTimeout(TimeSpan time)
            => GetHttp().WithTimeout(time);

        /// <summary>
        /// Only the response headers should be read, not the body. The body
        /// will be read later, once actually requested from the response
        /// object. After using this setup, you should access the response
        /// body via the asynchronous methods to avoid synchronous waiting.
        /// </summary>
        /// <returns>PendingRequest - the fluent API request builder</returns>
        public static PendingRequest WithoutResponseBuffering()
            => GetHttp().WithoutResponseBuffering();
        
        #endregion
        
        #region "Faking"
        
        /// <summary>
        /// Intercept all requests to make them testable
        /// </summary>
        /// <returns>Itself for chaining</returns>
        public static IHttp Fake()
            => GetHttp().Fake();

        /// <summary>
        /// Intercept all requests going to a matching URL
        /// </summary>
        /// <param name="urlPattern">Wildcard pattern with asterisks</param>
        /// <returns>Itself for chaining</returns>
        public static IHttp Fake(string urlPattern)
            => GetHttp().Fake(urlPattern);

        /// <summary>
        /// Intercept all requests and respond with the given response
        /// </summary>
        /// <param name="response"></param>
        /// <returns>Itself for chaining</returns>
        public static IHttp Fake(Response response)
            => GetHttp().Fake(response);

        /// <summary>
        /// Intercept all requests going to a matching URL and respond
        /// with a given response
        /// </summary>
        /// <param name="urlPattern">Wildcard pattern with asterisks</param>
        /// <param name="response"></param>
        /// <returns>Itself for chaining</returns>
        public static IHttp Fake(string urlPattern, Response response)
            => GetHttp().Fake(urlPattern, response);

        /// <summary>
        /// Intercept all requests and respond with a sequence of responses
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns>Itself for chaining</returns>
        public static IHttp Fake(ResponseSequence sequence)
            => GetHttp().Fake(sequence);
        
        /// <summary>
        /// Intercept all requests going to a certain URL and respond
        /// with a sequence of responses
        /// </summary>
        /// <param name="urlPattern">Wildcard pattern with asterisks</param>
        /// <param name="sequence"></param>
        /// <returns>Itself for chaining</returns>
        public static IHttp Fake(string urlPattern, ResponseSequence sequence)
            => GetHttp().Fake(urlPattern, sequence);

        /// <summary>
        /// Intercept all requests going to a certain URL and give them
        /// to a callback that may fake their response or do nothing
        /// if it returns null.
        /// </summary>
        /// <param name="urlPattern">Wildcard pattern with asterisks</param>
        /// <param name="callback"></param>
        /// <returns>Itself for chaining</returns>
        public static IHttp Fake(
            string urlPattern,
            Func<Request, Response> callback
        ) => GetHttp().Fake(urlPattern, callback);

        /// <summary>
        /// Intercept all requests and give them to a callback that may fake
        /// their response or do nothing if it returns null.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns>Itself for chaining</returns>
        public static IHttp Fake(Func<Request, Response> callback)
            => GetHttp().Fake(callback);
        
        #endregion
        
        #region "Recording"

        /// <summary>
        /// Returns all recorded request-response pairs
        /// </summary>
        public static List<RequestResponsePair> Recorded()
            => GetHttp().Recorded();
        
        /// <summary>
        /// Returns all recorded request-response pairs
        /// that fulfill the condition
        /// </summary>
        public static List<RequestResponsePair> Recorded(
            Func<Request, bool> condition
        ) => GetHttp().Recorded(condition);
        
        /// <summary>
        /// Returns all recorded request-response pairs
        /// that fulfill the condition
        /// </summary>
        public static List<RequestResponsePair> Recorded(
            Func<Request, Response, bool> condition
        ) => GetHttp().Recorded(condition);
        
        #endregion
        
        #region "Assertions"

        /// <summary>
        /// Throws a UnisaveAssertionException if there is no recorded request
        /// that matches the provided condition
        /// </summary>
        public static void AssertSent(Func<Request, bool> condition)
            => GetHttp().AssertSent(condition);
        
        /// <summary>
        /// Throws a UnisaveAssertionException if there is no recorded request
        /// that matches the provided condition
        /// </summary>
        public static void AssertSent(Func<Request, Response, bool> condition)
            => GetHttp().AssertSent(condition);
        
        /// <summary>
        /// Throws a UnisaveAssertionException if there is a recorded request
        /// that matches the provided condition
        /// </summary>
        public static void AssertNotSent(Func<Request, bool> condition)
            => GetHttp().AssertNotSent(condition);
        
        /// <summary>
        /// Throws a UnisaveAssertionException if there is a recorded request
        /// that matches the provided condition
        /// </summary>
        public static void AssertNotSent(
            Func<Request, Response, bool> condition
        ) => GetHttp().AssertNotSent(condition);
        
        /// <summary>
        /// Throws a UnisaveAssertionException if there is a recorded request
        /// </summary>
        public static void AssertNothingSent()
            => GetHttp().AssertNothingSent();
        
        #endregion
        
        #region "Stubbing"

        /// <summary>
        /// Creates a stub JSON response, used for testing
        /// </summary>
        /// <param name="json">Response JSON body</param>
        /// <param name="status">HTTP status code</param>
        /// <param name="headers">Additional response headers</param>
        /// <returns>Stub HTTP response object</returns>
        public static Response Response(
            JsonObject json,
            int status = 200,
            Dictionary<string, string> headers = null
        ) => GetHttp().Response(json, status, headers);

        /// <summary>
        /// Creates a stub string response, used for testing
        /// </summary>
        /// <param name="body">Response body</param>
        /// <param name="contentType">Text MIME type</param>
        /// <param name="status">HTTP response status code</param>
        /// <param name="headers">Additional response headers</param>
        /// <returns>Stub HTTP response object</returns>
        public static Response Response(
            string body,
            string contentType = "text/plain",
            int status = 200,
            Dictionary<string, string> headers = null
        ) => GetHttp().Response(body, contentType, status, headers);

        /// <summary>
        /// Creates a stub response, used for testing
        /// </summary>
        /// <param name="body">Response body</param>
        /// <param name="status">HTTP status code</param>
        /// <param name="headers">Response headers</param>
        /// <returns>Stub HTTP response object</returns>
        /// <exception cref="ArgumentException">Invalid headers</exception>
        public static Response Response(
            HttpContent body = null,
            int status = 200,
            Dictionary<string, string> headers = null
        ) => GetHttp().Response(body, status, headers);

        /// <summary>
        /// Creates a stub response sequence, used for testing
        /// </summary>
        /// <returns>
        /// Fluent builder for a sequence of stub HTTP responses
        /// </returns>
        public static ResponseSequence Sequence()
            => GetHttp().Sequence();

        #endregion
    }
}