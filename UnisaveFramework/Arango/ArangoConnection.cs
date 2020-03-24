using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using LightJson;
using LightJson.Serialization;
using Unisave.Arango.Query;
using Unisave.Contracts;

namespace Unisave.Arango
{
    /// <summary>
    /// HTTP connection to a real arango database
    /// </summary>
    public class ArangoConnection : IArango, IDisposable
    {
        public HttpClient Client { get; }
        public string BaseUrl { get; }
        public string Database { get; }
        public string Username { get; }
        public string Password { get; }
        
        public ArangoConnection(
            string baseUrl,
            string database,
            string username,
            string password
        )
        {
            Client = new HttpClient();
            BaseUrl = baseUrl;
            Database = database;
            Username = username;
            Password = password;
        }
        
        public void Dispose()
        {
            Client.Dispose();
        }
        
        #region "HTTP level API"

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
        
        public JsonObject Put(string url)
        {
            return WrapRequest(() => Client
                .PutAsync(BuildUrl(url), JsonContent(new JsonObject()))
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
            if (url == null)
                throw new ArgumentNullException(nameof(url));
            
            if (!url.StartsWith("/"))
                url = "/" + url;

            string baseUrl = BaseUrl;
            if (!baseUrl.EndsWith("/"))
                baseUrl += "/";

            return new Uri(
                baseUrl + "_db/" + Uri.EscapeUriString(Database) + url
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

            if (response.Content == null)
                return new JsonObject();
            
            string content = response.Content
                .ReadAsStringAsync()
                .GetAwaiter()
                .GetResult();
            
            if (string.IsNullOrEmpty(content))
                return new JsonObject();

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
        
        #endregion

        public List<JsonValue> ExecuteAqlQuery(AqlQuery query)
        {
            var results = new List<JsonValue>();
            
            string aql = query.ToAql();
            
            // create cursor
            JsonObject response = Post("/_api/cursor", new JsonObject()
                .Add("query", aql)
            );

            // may be null if the response is short
            string cursorId = response["id"].AsString;

            // get the first batch of results
            foreach (JsonValue item in response["result"].AsJsonArray)
                results.Add(item);
            
            // get all the remaining batches
            while (response["hasMore"])
            {
                response = Put("/_api/cursor/" + Uri.EscapeUriString(cursorId));
                
                foreach (JsonValue item in response["result"].AsJsonArray)
                    results.Add(item);
            }

            return results;
        }

        public void CreateCollection(string collectionName, CollectionType type)
        {
            Post("/_api/collection", new JsonObject()
                .Add("name", collectionName)
                .Add("waitForSync", false)
                .Add("isSystem", false)
                .Add("type", (int)type)
            );
        }

        public void DeleteCollection(string collectionName)
        {
            Delete("/_api/collection/" + Uri.EscapeUriString(collectionName));
        }
    }
}