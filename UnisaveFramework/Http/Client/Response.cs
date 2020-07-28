using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using LightJson;
using LightJson.Serialization;

namespace Unisave.Http.Client
{
    public class Response
    {
        // TODO: indexer access to JSON content -> cache parsed content
        
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
        /// Status code >= 400
        /// </summary>
        public bool Failed => Status >= 400;

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
        
        public JsonValue this[string key] => JsonValue.Null;
        
        public Response(HttpResponseMessage original)
        {
            Original = original;
        }
        
        #region "Stub response construction"

        /// <summary>
        /// Creates a stub JSON response, used for testing
        /// </summary>
        /// <param name="json">Response JSON body</param>
        /// <param name="status">HTTP status code</param>
        /// <param name="headers">Additional response headers</param>
        /// <returns></returns>
        public static Response Create(
            JsonObject json,
            int status = 200,
            Dictionary<string, string> headers = null
        )
        {
            return Create(
                json.ToString(),
                "application/json",
                status,
                headers
            );
        }

        /// <summary>
        /// Creates a stub string response, used for testing
        /// </summary>
        /// <param name="body">Response body</param>
        /// <param name="contentType">Text MIME type</param>
        /// <param name="status">HTTP response status code</param>
        /// <param name="headers">Additional response headers</param>
        /// <returns></returns>
        public static Response Create(
            string body,
            string contentType = "text/plain",
            int status = 200,
            Dictionary<string, string> headers = null
        )
        {
            return Create(
                new StringContent(body, Encoding.UTF8, contentType),
                status,
                headers
            );
        }

        /// <summary>
        /// Creates a stub response, used for testing
        /// </summary>
        /// <param name="body">Response body</param>
        /// <param name="status">HTTP status code</param>
        /// <param name="headers">Response headers</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Invalid headers</exception>
        public static Response Create(
            HttpContent body = null,
            int status = 200,
            Dictionary<string, string> headers = null
        )
        {
            var response = new HttpResponseMessage {
                Content = body,
                StatusCode = (HttpStatusCode) status,
            };

            if (headers != null)
            {
                foreach (var pair in headers)
                {
                    if (!response.Headers
                        .TryAddWithoutValidation(pair.Key, pair.Value))
                    {
                        throw new ArgumentException(
                            $"The header {pair.Key} cannot be specified via " +
                            $"the method {nameof(Create)}. The header " +
                            $"probably refers to the content and will " +
                            $"be determined automatically."
                        );
                    }
                }
            }
                
            return new Response(response);
        }
        
        #endregion

        /// <summary>
        /// Returns the response body as string.
        /// If the response is not a string or there is no response,
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
        /// Returns the response body as a parsed JSON object,
        /// or null if the response does not exist.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="JsonParseException"></exception>
        public JsonObject Json()
        {
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

            return json.AsJsonObject;
        }

        /// <summary>
        /// Throws an exception if the response is 400 or above.
        /// This method is chainable.
        /// </summary>
        /// <exception cref="HttpRequestException"></exception>
        public Response Throw()
        {
            if (IsClientError || IsServerError)
                throw new HttpRequestException(
                    "The HTTP response has erroneous status code: " +
                    (int)Original.StatusCode
                );

            return this;
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