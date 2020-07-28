using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using LightJson;
using LightJson.Serialization;

namespace Unisave.Http.Client
{
    public class Request
    {
        // TODO: indexer access to JSON payload
        
        /// <summary>
        /// The original request message from the .NET framework
        /// </summary>
        public HttpRequestMessage Original { get; }

        /// <summary>
        /// URL of the request
        /// </summary>
        public string Url => Original.RequestUri.ToString();
        
        // caching JSON body for fast indexer access
        private JsonObject jsonCache;
        private bool jsonCacheUsed = false;
        
        /// <summary>
        /// Access the body as a JSON object
        /// </summary>
        /// <param name="key"></param>
        public JsonValue this[string key] => Json()[key];
        
        public Request(HttpRequestMessage original)
        {
            Original = original;
        }
        
        /// <summary>
        /// Returns the request body as string.
        /// If the request is not a string or there is no request body,
        /// null is returned.
        /// </summary>
        /// <returns></returns>
        public string Body()
        {
            var content = Original.Content as StringContent;

            if (content == null)
                return null;
            
            return content.ReadAsStringAsync()
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Returns the request body as a parsed JSON object,
        /// or null if the request does not exist.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="JsonParseException"></exception>
        public JsonObject Json()
        {
            if (jsonCacheUsed)
                return jsonCache;
            
            var content = Original.Content as StringContent;

            if (content == null)
                return null;
            
            if (content.Headers.ContentType.MediaType != "application/json")
            {
                throw new InvalidOperationException(
                    "The response is not application/json but: " +
                    content.Headers.ContentType.MediaType 
                );
            }

            string jsonString = content.ReadAsStringAsync()
                .GetAwaiter()
                .GetResult();

            JsonValue json = JsonReader.Parse(jsonString);
            
            if (!json.IsJsonObject)
                throw new InvalidOperationException(
                    "The response body is not a JSON object"
                );

            jsonCache = json.AsJsonObject;
            jsonCacheUsed = true;
            return jsonCache;
        }
        
        /// <summary>
        /// Get value of a header or null if header not present
        /// </summary>
        /// <param name="name">Header name</param>
        /// <returns></returns>
        public string Header(string name)
        {
            IEnumerable<string> values;
        
            if (Original.Headers.TryGetValues(name, out values))
                return values.FirstOrDefault();
            
            if (Original.Content != null &&
                Original.Content.Headers.TryGetValues(name, out values))
                return values.FirstOrDefault();

            return null;
        }

        /// <summary>
        /// Returns true if a given header is present
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool HasHeader(string name)
            => Header(name) != null;
        
        /// <summary>
        /// Returns true if a given header has the given value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool HasHeader(string name, string value)
            => Header(name) == value;
    }
}