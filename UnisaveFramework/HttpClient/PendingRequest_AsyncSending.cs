#nullable enable
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
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
        public Task<Response> GetAsync(
            string url,
            Dictionary<string, string>? query = null
        )
        {
            return SendAsync(HttpMethod.Get, url, query);
        }

        /// <summary>
        /// Sends a POST request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
        public Task<Response> PostAsync(
            string url,
            Dictionary<string, string> form
        )
        {
            WithFormBody(form);
            
            return SendAsync(HttpMethod.Post, url);
        }
        
        /// <summary>
        /// Sends a POST request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns>The HTTP response object</returns>
        public Task<Response> PostAsync(string url, JsonObject json)
        {
            WithJsonBody(json);
            
            return SendAsync(HttpMethod.Post, url);
        }
        
        /// <summary>
        /// Sends a POST request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns>The HTTP response object</returns>
        public Task<Response> PostAsync(string url)
        {
            return SendAsync(HttpMethod.Post, url);
        }
        
        /// <summary>
        /// Sends a PUT request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
        public Task<Response> PutAsync(
            string url,
            Dictionary<string, string> form
        )
        {
            WithFormBody(form);
            
            return SendAsync(HttpMethod.Put, url);
        }
        
        /// <summary>
        /// Sends a PUT request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns>The HTTP response object</returns>
        public Task<Response> PutAsync(string url, JsonObject json)
        {
            WithJsonBody(json);
            
            return SendAsync(HttpMethod.Put, url);
        }
        
        /// <summary>
        /// Sends a PUT request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns>The HTTP response object</returns>
        public Task<Response> PutAsync(string url)
        {
            return SendAsync(HttpMethod.Put, url);
        }
        
        /// <summary>
        /// Sends a PATCH request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
        public Task<Response> PatchAsync(
            string url,
            Dictionary<string, string> form
        )
        {
            WithFormBody(form);
            
            return SendAsync(new HttpMethod("PATCH"), url);
        }
        
        /// <summary>
        /// Sends a PATCH request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns>The HTTP response object</returns>
        public Task<Response> PatchAsync(string url, JsonObject json)
        {
            WithJsonBody(json);
            
            return SendAsync(new HttpMethod("PATCH"), url);
        }
        
        /// <summary>
        /// Sends a PATCH request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns>The HTTP response object</returns>
        public Task<Response> PatchAsync(string url)
        {
            return SendAsync(new HttpMethod("PATCH"), url);
        }
        
        /// <summary>
        /// Sends a DELETE request with form url encoded content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="form">Content</param>
        /// <returns>The HTTP response object</returns>
        public Task<Response> DeleteAsync(
            string url,
            Dictionary<string, string> form
        )
        {
            WithFormBody(form);
            
            return SendAsync(HttpMethod.Delete, url);
        }
        
        /// <summary>
        /// Sends a DELETE request with JSON content
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <param name="json">Content</param>
        /// <returns>The HTTP response object</returns>
        public Task<Response> DeleteAsync(string url, JsonObject json)
        {
            WithJsonBody(json);
            
            return SendAsync(HttpMethod.Delete, url);
        }
        
        /// <summary>
        /// Sends a DELETE request
        /// (the content is empty unless specified by a previous command)
        /// </summary>
        /// <param name="url">Target URL</param>
        /// <returns>The HTTP response object</returns>
        public Task<Response> DeleteAsync(string url)
        {
            return SendAsync(HttpMethod.Delete, url);
        }
    }
}