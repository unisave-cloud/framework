using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using LightJson;
using LightJson.Serialization;

namespace Unisave.Arango
{
    /// <summary>
    /// HTTP connection to a real arango database
    /// </summary>
    public class ArangoConnection : IDisposable
    {
        public HttpClient Client { get; }
        public string BaseUrl { get; }
        public string Database { get; }
        public string Username { get; }
        public string Password { get; }
        
        public ArangoConnection()
        {
            Client = new HttpClient();
            BaseUrl = "http://127.0.0.1:8529/";
            Database = "_system";
            Username = "root";
            Password = "password";
        }

        public JsonObject Get(string url)
        {
            return WrapRequest(() => Client
                .GetAsync(BuildUrl(url))
                .GetAwaiter()
                .GetResult()
            );
        }
        
        public JsonObject Post(string url, JsonValue payload)
        {
            return WrapRequest(() => Client
                .PostAsync(BuildUrl(url), JsonContent(payload))
                .GetAwaiter()
                .GetResult()
            );
        }
        
        public JsonObject Put(string url, JsonValue payload)
        {
            return WrapRequest(() => Client
                .PutAsync(BuildUrl(url), JsonContent(payload))
                .GetAwaiter()
                .GetResult()
            );
        }
        
        public JsonObject Delete(string url)
        {
            return WrapRequest(() => Client
                .DeleteAsync(BuildUrl(url))
                .GetAwaiter()
                .GetResult()
            );
        }

        public HttpContent JsonContent(JsonValue json)
        {
            return new StringContent(
                json.ToString(),
                Encoding.UTF8,
                "application/json"
            );
        }

        /// <summary>
        /// Builds the URL, given the last arango portion
        /// </summary>
        public Uri BuildUrl(string url)
        {
            return new Uri(
                new Uri(
                    new Uri(BaseUrl),
                    "/_db/" + Uri.EscapeUriString(Database) + "/"
                ),
                url
            );
        }

        protected JsonObject WrapRequest(Func<HttpResponseMessage> action)
        {
            Client.DefaultRequestHeaders.Authorization
                = new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(
                        Encoding.ASCII.GetBytes(Username + ":" + Password)
                    )
                );

            HttpResponseMessage response = action.Invoke();
            
            string content = response.Content
                .ReadAsStringAsync()
                .GetAwaiter()
                .GetResult();

            JsonObject parsedContent = JsonReader.Parse(content).AsJsonObject;

            if (parsedContent["error"])
            {
                throw new ArangoException(
                    parsedContent["code"],
                    parsedContent["errorNum"],
                    parsedContent["errorMessage"]
                );
            }

            return parsedContent;
        }

        public void Dispose()
        {
            Client.Dispose();
        }
    }
}