#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LightJson;
using LightJson.Serialization;
using Unisave.Utils;

namespace Unisave.HttpClient
{
    /// <summary>
    /// Base class for Request and Response, and it provides
    /// methods for accessing the content of the request / response
    /// </summary>
    public abstract class HttpContentHolder
    {
        // NOTE: It is not as important to use async API here as most HTTP
        // response bodies are small and arrive right after the header.
        // Therefore, this class can be used via the synchronous API,
        // unless the response is expected to either be long or slowly sent out,
        // in which case you can use the BodyAsync, BytesAsync,
        // or even StreamAsync methods. Alternatively you can access the
        // underlying HttpContent if you need more control than that
        // (e.g. to consume Server Sent Events and other complex responses)
        
        /// <summary>
        /// Gives the content holder access to the actual HttpContent
        /// </summary>
        protected abstract HttpContent? Content { get; }

        /// <summary>
        /// Is there a body present at all?
        /// </summary>
        public bool HasBody => Content != null
            && Content.GetType().Name != "EmptyContent";
            // dotnet creates this instance, when you sent the content of
            // an http response to null

        /// <summary>
        /// Is the body a JSON body?
        /// </summary>
        public bool HasJsonBody => HasBody
            && Content!.Headers.ContentType.MediaType == "application/json";
        
        /// <summary>
        /// Is the body a form body?
        /// </summary>
        public bool HasFormBody => HasBody
            && Content!.Headers.ContentType.MediaType
                == "application/x-www-form-urlencoded";
        
        // caching body for fast indexer access
        private JsonObject? jsonCache;
        private Dictionary<string, string>? formCache;
        
        /// <summary>
        /// Access the body as a JSON object
        /// </summary>
        /// <param name="key"></param>
        public JsonValue this[string key]
        {
            get
            {
                if (HasJsonBody)
                    return (Json() ?? new JsonObject())[key];

                if (HasFormBody)
                    return (Form() ?? new Dictionary<string, string>())[key];
                
                throw new InvalidOperationException(
                    "Body is not JSON, nor url form encoded and so cannot " +
                    "be accessed via the indexer."
                );
            }
        }

        /// <summary>
        /// Returns the request body as string.
        /// If there is no request body, null is returned.
        /// </summary>
        public async Task<string?> BodyAsync()
        {
            if (!HasBody)
                return null;

            return await Content!.ReadAsStringAsync();
        }

        /// <summary>
        /// Returns the request body as string.
        /// If there is no request body, null is returned.
        /// </summary>
        public string? Body()
        {
            return UnisaveConcurrency.WaitForTask(BodyAsync);
        }

        /// <summary>
        /// Returns the request body as a byte array.
        /// If there is no request body, null is returned.
        /// </summary>
        public async Task<byte[]?> BytesAsync()
        {
            if (!HasBody)
                return null;

            return await Content!.ReadAsByteArrayAsync();
        }
        
        /// <summary>
        /// Returns the request body as a byte array.
        /// If there is no request body, null is returned.
        /// </summary>
        public byte[]? Bytes()
        {
            return UnisaveConcurrency.WaitForTask(BytesAsync);
        }

        /// <summary>
        /// Returns the request body as a Stream object.
        /// If there is no request body, null is returned.
        /// Use this in conjunction with the WithoutResponseBuffering() method
        /// on the PendingRequest.
        /// </summary>
        public async Task<Stream?> StreamAsync()
        {
            if (!HasBody)
                return null;

            return await Content!.ReadAsStreamAsync();
        }
        
        /// <summary>
        /// Returns the request body as a Stream object.
        /// If there is no request body, null is returned.
        /// Use this in conjunction with the WithoutResponseBuffering() method
        /// on the PendingRequest.
        /// </summary>
        public Stream? Stream()
        {
            return UnisaveConcurrency.WaitForTask(StreamAsync);
        }

        /// <summary>
        /// Returns the body as a parsed JSON object,
        /// or null if the body does not exist.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="JsonParseException"></exception>
        public async Task<JsonObject?> JsonAsync()
        {
            if (jsonCache != null)
                return jsonCache;

            JsonValue json = await JsonValueAsync();

            if (json.IsNull)
                return null;
            
            if (!json.IsJsonObject)
                throw new InvalidOperationException(
                    "The body is not a JSON object"
                );

            jsonCache = json.AsJsonObject;
            return jsonCache;
        }

        /// <summary>
        /// Returns the body as a parsed JSON object,
        /// or null if the body does not exist.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="JsonParseException"></exception>
        public JsonObject? Json()
        {
            return UnisaveConcurrency.WaitForTask(JsonAsync);
        }

        /// <summary>
        /// Returns the body as a parsed JSON array,
        /// or null if the body does not exist.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="JsonParseException"></exception>
        public async Task<JsonArray?> JsonArrayAsync()
        {
            JsonValue json = await JsonValueAsync();
            
            if (json.IsNull)
                return null;
            
            if (!json.IsJsonArray)
                throw new InvalidOperationException(
                    "The body is not a JSON array"
                );

            return json.AsJsonArray;
        }

        /// <summary>
        /// Returns the body as a parsed JSON array,
        /// or null if the body does not exist.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="JsonParseException"></exception>
        public JsonArray? JsonArray()
        {
            return UnisaveConcurrency.WaitForTask(JsonArrayAsync);
        }

        /// <summary>
        /// Returns the body as a parsed JSON value,
        /// or JsonValue.Null if the body is null or doesn't exist
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="JsonParseException"></exception>
        public async Task<JsonValue> JsonValueAsync()
        {
            if (!HasBody)
                return LightJson.JsonValue.Null;
            
            if (Content!.Headers.ContentType.MediaType != "application/json")
            {
                throw new InvalidOperationException(
                    "The content type is not application/json but: " +
                    Content.Headers.ContentType.MediaType 
                );
            }

            string? jsonString = await BodyAsync();
            
            if (jsonString == null)
                return LightJson.JsonValue.Null;

            return JsonReader.Parse(jsonString);
        }

        /// <summary>
        /// Returns the body as a parsed JSON value,
        /// or JsonValue.Null if the body is null or doesn't exist
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="JsonParseException"></exception>
        public JsonValue JsonValue()
        {
            return UnisaveConcurrency.WaitForTask(JsonValueAsync);
        }

        /// <summary>
        /// Returns the body as a parsed url form encoded dictionary,
        /// or null if the body does not exist.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<Dictionary<string, string>?> FormAsync()
        {
            if (formCache != null)
                return formCache;
            
            if (!HasBody)
                return null;
            
            if (Content!.Headers.ContentType.MediaType !=
                "application/x-www-form-urlencoded")
            {
                throw new InvalidOperationException(
                    "The body is not of type " +
                    "application/x-www-form-urlencoded but: " +
                    Content.Headers.ContentType.MediaType 
                );
            }

            string? formString = await BodyAsync();
            
            if (formString == null)
                return null;

            var form = formString.Split('&')
                .Select(p => p.Split('='))
                .ToDictionary(
                    p => WebUtility.UrlDecode(p[0]),
                    p => WebUtility.UrlDecode(p[1])
                );

            formCache = form;
            return formCache;
        }

        /// <summary>
        /// Returns the body as a parsed url form encoded dictionary,
        /// or null if the body does not exist.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public Dictionary<string, string>? Form()
        {
            return UnisaveConcurrency.WaitForTask(FormAsync);
        }
    }
}