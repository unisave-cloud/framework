using System;
using System.Collections.Generic;
using System.Net.Http;
using LightJson;
using Unisave.HttpClient;

namespace Unisave.Facades
{
    /*
     * Why create custom synchronous HTTP client,
     * when .NET gives you "HttpClient" class?
     *
     * - Unisave backend should be single-threaded and synchronous,
     *     which plays nicely with the request-based structure of the system
     * - We can easily fake requests, which is useful for testing
     * - HttpClient from .NET lacks proper JSON integration
     * - Fluent API (inspired by Laravel)
     */
    
    /// <summary>
    /// Facade for making HTTP requests
    /// </summary>
    public static class Http
    {
        private static Factory GetFactory()
        {
            if (!Facade.HasApp)
                throw new InvalidOperationException(
                    "You cannot use the Http facade the client side."
                );
            
            return Facade.App.Services.Resolve<Factory>();
        }
        
        #region "Request construction"
        
        /// <summary>
        /// Creates a new instance of pending request
        /// </summary>
        /// <returns></returns>
        public static PendingRequest Request()
            => GetFactory().PendingRequest();

        /// <summary>
        /// Sets additional request headers
        /// </summary>
        /// <param name="requestHeaders"></param>
        /// <returns></returns>
        public static PendingRequest WithHeaders(
            Dictionary<string, string> requestHeaders
        ) => Request().WithHeaders(requestHeaders);

        /// <summary>
        /// Specifies the request body via the .NET HttpContent class.
        /// Null represent no body.
        /// </summary>
        /// <param name="body"></param>
        /// <returns></returns>
        public static PendingRequest WithBody(HttpContent body)
            => Request().WithBody(body);

        /// <summary>
        /// Specifies the request body as a form url encoded content
        /// </summary>
        /// <param name="form">Content</param>
        /// <returns></returns>
        public static PendingRequest WithFormBody(Dictionary<string, string> form)
            => Request().WithFormBody(form);

        /// <summary>
        /// Specifies the request body as a JSON object
        /// </summary>
        /// <param name="json">The JSON object</param>
        /// <returns></returns>
        public static PendingRequest WithJsonBody(JsonObject json)
            => Request().WithJsonBody(json);

        /// <summary>
        /// Attaches a JSON part to the multipart content
        /// </summary>
        /// <param name="name">Part name</param>
        /// <param name="jsonPart">Json content</param>
        /// <param name="fileName">Filename if the part is a file</param>
        /// <param name="contentHeaders">Additional part headers</param>
        /// <returns></returns>
        public static PendingRequest Attach(
            string name,
            JsonObject jsonPart,
            string fileName = null,
            Dictionary<string, string> contentHeaders = null
        ) => Request().Attach(name, jsonPart, fileName, contentHeaders);

        /// <summary>
        /// Attaches a part to the multipart content
        /// </summary>
        /// <param name="name">Part name</param>
        /// <param name="part">Part content</param>
        /// <param name="fileName">Filename if the part is a file</param>
        /// <param name="contentHeaders">Additional part headers</param>
        /// <returns></returns>
        public static PendingRequest Attach(
            string name,
            HttpContent part,
            string fileName = null,
            Dictionary<string, string> contentHeaders = null
        ) => Request().Attach(name, part, fileName, contentHeaders);

        /// <summary>
        /// Adds authentication data to the request
        /// (basic authentication from RFC 7617)
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static PendingRequest WithBasicAuth(
            string username,
            string password
        ) => Request().WithBasicAuth(username, password);

        /// <summary>
        /// Adds authentication data to the request
        /// (bearer authentication of OAuth 2.0 from RFC 6750)
        /// </summary>
        /// <param name="bearerToken"></param>
        /// <returns></returns>
        public static PendingRequest WithToken(string bearerToken)
            => Request().WithToken(bearerToken);
        
        #endregion
        
        #region "Request sending"

        /// <summary>
        /// Sends a GET request
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="query">Query parameters</param>
        /// <returns></returns>
        public static Response Get(
            string url,
            Dictionary<string, string> query = null
        ) => Request().Get(url, query);

        /// <summary>
        /// Sends a POST request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns></returns>
        public static Response Post(string url, Dictionary<string, string> form)
            => Request().Post(url, form);
        
        /// <summary>
        /// Sends a POST request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns></returns>
        public static Response Post(string url, JsonObject json)
            => Request().Post(url, json);
        
        /// <summary>
        /// Sends a POST request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns></returns>
        public static Response Post(string url)
            => Request().Post(url);
        
        /// <summary>
        /// Sends a PUT request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns></returns>
        public static Response Put(string url, Dictionary<string, string> form)
            => Request().Put(url, form);
        
        /// <summary>
        /// Sends a PUT request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns></returns>
        public static Response Put(string url, JsonObject json)
            => Request().Put(url, json);
        
        /// <summary>
        /// Sends a PUT request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns></returns>
        public static Response Put(string url)
            => Request().Put(url);
        
        /// <summary>
        /// Sends a PATCH request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns></returns>
        public static Response Patch(string url, Dictionary<string, string> form)
            => Request().Patch(url, form);
        
        /// <summary>
        /// Sends a PATCH request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns></returns>
        public static Response Patch(string url, JsonObject json)
            => Request().Patch(url, json);
        
        /// <summary>
        /// Sends a PATCH request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns></returns>
        public static Response Patch(string url)
            => Request().Patch(url);
        
        /// <summary>
        /// Sends a DELETE request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns></returns>
        public static Response Delete(string url, Dictionary<string, string> form)
            => Request().Delete(url, form);
        
        /// <summary>
        /// Sends a DELETE request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns></returns>
        public static Response Delete(string url, JsonObject json)
            => Request().Delete(url, json);
        
        /// <summary>
        /// Sends a DELETE request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns></returns>
        public static Response Delete(string url)
            => Request().Delete(url);
        
        /// <summary>
        /// Sends an HTTP request with the method specified as a parameter
        /// </summary>
        /// <param name="method">HTTP method</param>
        /// <param name="url">Target URL</param>
        /// <param name="query">Query string to put into the URL</param>
        /// <returns></returns>
        public static Response Send(
            HttpMethod method,
            string url,
            Dictionary<string, string> query = null
        ) => Request().Send(method, url, query);
        
        #endregion
        
        #region "Faking"
        
        /// <summary>
        /// Intercept all requests to make them testable
        /// </summary>
        /// <returns></returns>
        public static Factory Fake()
            => GetFactory().Fake();

        /// <summary>
        /// Intercept all requests going to a matching URL
        /// </summary>
        /// <param name="urlPattern">Wildcard pattern with asterisks</param>
        /// <returns></returns>
        public static Factory Fake(string urlPattern)
            => GetFactory().Fake(urlPattern);

        /// <summary>
        /// Intercept all requests and respond with the given response
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public static Factory Fake(Response response)
            => GetFactory().Fake(response);

        /// <summary>
        /// Intercept all requests going to a matching URL and respond
        /// with a given response
        /// </summary>
        /// <param name="urlPattern">Wildcard pattern with asterisks</param>
        /// <param name="response"></param>
        /// <returns></returns>
        public static Factory Fake(string urlPattern, Response response)
            => GetFactory().Fake(urlPattern, response);

        /// <summary>
        /// Intercept all requests and respond with a sequence of responses
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public static Factory Fake(ResponseSequence sequence)
            => GetFactory().Fake(sequence);
        
        /// <summary>
        /// Intercept all requests going to a certain URL and respond
        /// with a sequence of responses
        /// </summary>
        /// <param name="urlPattern">Wildcard pattern with asterisks</param>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public static Factory Fake(string urlPattern, ResponseSequence sequence)
            => GetFactory().Fake(urlPattern, sequence);

        /// <summary>
        /// Intercept all requests going to a certain URL and give them
        /// to a callback that may fake their response or do nothing
        /// if it returns null.
        /// </summary>
        /// <param name="urlPattern">Wildcard pattern with asterisks</param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static Factory Fake(
            string urlPattern,
            Func<Request, Response> callback
        ) => GetFactory().Fake(urlPattern, callback);

        /// <summary>
        /// Intercept all requests and give them to a callback that may fake
        /// their response or do nothing if it returns null.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static Factory Fake(Func<Request, Response> callback)
            => GetFactory().Fake(callback);
        
        #endregion
        
        #region "Recording"

        /// <summary>
        /// Returns all recorded request-response pairs
        /// </summary>
        public static List<Factory.Record> Recorded()
            => GetFactory().Recorded();
        
        /// <summary>
        /// Returns all recorded request-response pairs
        /// that fulfill the condition
        /// </summary>
        public static List<Factory.Record> Recorded(
            Func<Request, bool> condition
        ) => GetFactory().Recorded(condition);
        
        /// <summary>
        /// Returns all recorded request-response pairs
        /// that fulfill the condition
        /// </summary>
        public static List<Factory.Record> Recorded(
            Func<Request, Response, bool> condition
        ) => GetFactory().Recorded(condition);
        
        #endregion
        
        #region "Assertions"

        public static void AssertSent(Func<Request, bool> condition)
            => GetFactory().AssertSent(condition);
        
        public static void AssertSent(Func<Request, Response, bool> condition)
            => GetFactory().AssertSent(condition);
        
        public static void AssertNotSent(Func<Request, bool> condition)
            => GetFactory().AssertNotSent(condition);
        
        public static void AssertNotSent(Func<Request, Response, bool> condition)
            => GetFactory().AssertNotSent(condition);
        
        public static void AssertNothingSent()
            => GetFactory().AssertNothingSent();
        
        #endregion
        
        #region "Stubbing"

        /// <summary>
        /// Creates a stub JSON response, used for testing
        /// </summary>
        /// <param name="json">Response JSON body</param>
        /// <param name="status">HTTP status code</param>
        /// <param name="headers">Additional response headers</param>
        /// <returns></returns>
        public static Response Response(
            JsonObject json,
            int status = 200,
            Dictionary<string, string> headers = null
        )
            => HttpClient.Response.Create(
                json, status, headers
            );

        /// <summary>
        /// Creates a stub string response, used for testing
        /// </summary>
        /// <param name="body">Response body</param>
        /// <param name="contentType">Text MIME type</param>
        /// <param name="status">HTTP response status code</param>
        /// <param name="headers">Additional response headers</param>
        /// <returns></returns>
        public static Response Response(
            string body,
            string contentType = "text/plain",
            int status = 200,
            Dictionary<string, string> headers = null
        )
            => HttpClient.Response.Create(
                body, contentType, status, headers
            );

        /// <summary>
        /// Creates a stub response, used for testing
        /// </summary>
        /// <param name="body">Response body</param>
        /// <param name="status">HTTP status code</param>
        /// <param name="headers">Response headers</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Invalid headers</exception>
        public static Response Response(
            HttpContent body = null,
            int status = 200,
            Dictionary<string, string> headers = null
        )
            => HttpClient.Response.Create(
                body, status, headers
            );

        /// <summary>
        /// Creates a stub response sequence, used for testing
        /// </summary>
        /// <returns></returns>
        public static ResponseSequence Sequence()
            => new ResponseSequence();

        #endregion
    }
}