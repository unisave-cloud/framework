#nullable enable
using System.Collections.Generic;
using System.Net.Http;
using LightJson;

namespace Unisave.HttpClient
{
    public partial class PendingRequest
    {
        /// <summary>
        /// Sends a GET request
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="query">Query parameters</param>
        /// <returns>The HTTP response object</returns>
        public Response Get(
            string url,
            Dictionary<string, string>? query = null
        )
        {
            return Send(HttpMethod.Get, url, query);
        }

        /// <summary>
        /// Sends a POST request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
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
        /// <returns>The HTTP response object</returns>
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
        /// <returns>The HTTP response object</returns>
        public Response Post(string url)
        {
            return Send(HttpMethod.Post, url);
        }
        
        /// <summary>
        /// Sends a PUT request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
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
        /// <returns>The HTTP response object</returns>
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
        /// <returns>The HTTP response object</returns>
        public Response Put(string url)
        {
            return Send(HttpMethod.Put, url);
        }
        
        /// <summary>
        /// Sends a PATCH request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
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
        /// <returns>The HTTP response object</returns>
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
        /// <returns>The HTTP response object</returns>
        public Response Patch(string url)
        {
            return Send(new HttpMethod("PATCH"), url);
        }
        
        /// <summary>
        /// Sends a DELETE request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
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
        /// <returns>The HTTP response object</returns>
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
        /// <returns>The HTTP response object</returns>
        public Response Delete(string url)
        {
            return Send(HttpMethod.Delete, url);
        }
    }
}