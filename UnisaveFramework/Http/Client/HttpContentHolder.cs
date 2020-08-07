using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using LightJson;
using LightJson.Serialization;

namespace Unisave.Http.Client
{
    /// <summary>
    /// Base class for Request and Response and it provides
    /// methods for accessing the content of the request / response
    /// </summary>
    public abstract class HttpContentHolder
    {
        /// <summary>
        /// Gives the content holder access to the actual HttpContent
        /// </summary>
        protected abstract HttpContent Content { get; }

        /// <summary>
        /// Is there a body present at all?
        /// </summary>
        public bool HasBody => Content != null;

        /// <summary>
        /// Is the body a JSON body?
        /// </summary>
        public bool HasJsonBody => HasBody
            && Content is StringContent
            && Content.Headers.ContentType.MediaType == "application/json";
        
        /// <summary>
        /// Is the body a form body?
        /// </summary>
        public bool HasFormBody => HasBody
            && Content is FormUrlEncodedContent;
        
        // caching body for fast indexer access
        private JsonObject jsonCache;
        private bool jsonCacheUsed = false;
        private Dictionary<string, string> formCache;
        private bool formCacheUsed = false;
        
        /// <summary>
        /// Access the body as a JSON object
        /// </summary>
        /// <param name="key"></param>
        public JsonValue this[string key]
        {
            get
            {
                if (HasJsonBody)
                    return Json()[key];

                if (HasFormBody)
                    return Form()[key];
                
                throw new InvalidOperationException(
                    "Body is not JSON, nor url form encoded and so cannot " +
                    "be accessed via the indexer."
                );
            }
        }

        /// <summary>
        /// Returns the request body as string.
        /// If the request is not a string or there is no request body,
        /// null is returned.
        /// </summary>
        /// <returns></returns>
        public string Body()
        {
            var content = Content as StringContent;

            if (content == null)
                return null;
            
            return content.ReadAsStringAsync()
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Returns the body as a parsed JSON object,
        /// or null if the body does not exist.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="JsonParseException"></exception>
        public JsonObject Json()
        {
            if (jsonCacheUsed)
                return jsonCache;
            
            var content = Content as StringContent;

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
        /// Returns the body as a parsed url form encoded dictionary,
        /// or null if the body does not exist.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Dictionary<string, string> Form()
        {
            if (formCacheUsed)
                return formCache;
            
            var content = Content as FormUrlEncodedContent;

            if (content == null)
                return null;

            string formString = content.ReadAsStringAsync()
                .GetAwaiter()
                .GetResult();

            var form = formString.Split('&')
                .Select(p => p.Split('='))
                .ToDictionary(
                    p => WebUtility.UrlDecode(p[0]),
                    p => WebUtility.UrlDecode(p[1])
                );

            formCache = form;
            formCacheUsed = true;
            return formCache;
        }
    }
}