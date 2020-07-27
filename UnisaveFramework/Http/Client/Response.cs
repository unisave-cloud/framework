using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using LightJson;
using LightJson.Serialization;

namespace Unisave.Http.Client
{
    public class Response
    {
        /// <summary>
        /// The original response message from the .NET framework
        /// </summary>
        public HttpResponseMessage Original { get; }

        /// <summary>
        /// Status code as integer
        /// </summary>
        public int Status => (int)Original.StatusCode;

        /// <summary>
        /// Is the status code 200?
        /// </summary>
        public bool IsOk => Original.StatusCode == HttpStatusCode.OK;

        /// <summary>
        /// Status code 2xx
        /// </summary>
        public bool IsSuccessful => Status >= 200 && Status < 300;

        /// <summary>
        /// Status code 3xx
        /// </summary>
        public bool IsRedirect => Status >= 400 && Status < 500;
        
        /// <summary>
        /// Status code 4xx
        /// </summary>
        public bool IsClientError => Status >= 400 && Status < 500;
        
        /// <summary>
        /// Status code 5xx
        /// </summary>
        public bool IsServerError => Status >= 500;
        
        public Response(HttpResponseMessage original)
        {
            Original = original;
        }

        /// <summary>
        /// Returns the response body as string
        /// (but the response has to be in a text format)
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public string Body()
        {
            var content = Original.Content as StringContent;

            if (content == null)
                throw new InvalidOperationException(
                    "The response does not contain text"
                );
            
            return content.ReadAsStringAsync()
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Returns the response body as a parsed JSON object
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public JsonObject Json()
        {
            var content = Original.Content as StringContent;

            if (content == null ||
                content.Headers.ContentType.MediaType != "application/json")
            {
                throw new InvalidOperationException(
                    "The response does not contain JSON body"
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

            return json.AsJsonObject;
        }

        /// <summary>
        /// Throws an exception if the response is 400 or above
        /// </summary>
        /// <exception cref="HttpRequestException"></exception>
        public void Throw()
        {
            if (IsClientError || IsServerError)
                throw new HttpRequestException(
                    "The HTTP response has erroneous status code: " +
                    (int)Original.StatusCode
                );
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
    }
}