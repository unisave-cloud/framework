using System;
using System.Collections.Generic;
using System.Net.Http;
using LightJson;
using Unisave.Http.Client;

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
        // TODO: Factory + PendingRequest API here + Response & ResponseSequence factories
        
        #region "Regular usage"
        #endregion
        
        #region "Testing"
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
            => Unisave.Http.Client.Response.Create(
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
            => Unisave.Http.Client.Response.Create(
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
            => Unisave.Http.Client.Response.Create(
                body, status, headers
            );
        
        #endregion
    }
}