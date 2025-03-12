using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using LightJson;
using LightJson.Serialization;
using Unisave.Arango.Query;
using Unisave.Contracts;
using Unisave.Utils;

namespace Unisave.Arango
{
    /// <summary>
    /// HTTP connection to a real arango database
    /// </summary>
    public class ArangoConnection : IArango, IDisposable
    {
        public System.Net.Http.HttpClient Client { get; }
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
            // store connection details (read-only immutable)
            BaseUrl = baseUrl;
            Database = database;
            Username = username;
            Password = password;
            
            // create the HttpClient instance that will be used to request the DB 
            Client = new System.Net.Http.HttpClient();
            
            // specify the authentication header for each database request
            Client.DefaultRequestHeaders.Authorization
                = new AuthenticationHeaderValue(
                    "Basic",
                    Convert.ToBase64String(
                        Encoding.ASCII.GetBytes(Username + ":" + Password)
                    )
                );
        }
        
        public void Dispose()
        {
            Client.Dispose();
        }
        
        #region "HTTP level API"

        public JsonObject Get(string url)
        {
            return UnisaveConcurrency.WaitForTask(
                () => GetAsync(url)
            );
        }
        
        public async Task<JsonObject> GetAsync(string url)
        {
            return ParseResponse(
                await Client.GetAsync(BuildUrl(url))
            );
        }
        
        public JsonObject Post(string url, JsonValue payload)
        {
            return UnisaveConcurrency.WaitForTask(
                () => PostAsync(url, payload)
            );
        }
        
        public async Task<JsonObject> PostAsync(string url, JsonValue payload)
        {
            return ParseResponse(
                await Client.PostAsync(BuildUrl(url), JsonContent(payload))
            );
        }
        
        public JsonObject Put(string url, JsonValue payload)
        {
            return UnisaveConcurrency.WaitForTask(
                () => PutAsync(url, payload)
            );
        }
        
        public async Task<JsonObject> PutAsync(string url, JsonValue payload)
        {
            return ParseResponse(
                await Client.PutAsync(BuildUrl(url), JsonContent(payload))
            );
        }
        
        public JsonObject Put(string url)
        {
            return UnisaveConcurrency.WaitForTask(
                () => PutAsync(url)
            );
        }
        
        public async Task<JsonObject> PutAsync(string url)
        {
            return ParseResponse(
                await Client.PutAsync(
                    BuildUrl(url),
                    JsonContent(new JsonObject())
                )
            );
        }
        
        public JsonObject Delete(string url)
        {
            return UnisaveConcurrency.WaitForTask(
                () => DeleteAsync(url)
            );
        }
        
        public async Task<JsonObject> DeleteAsync(string url)
        {
            return ParseResponse(
                await Client.DeleteAsync(BuildUrl(url))
            );
        }

        private HttpContent JsonContent(JsonValue json)
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
        private Uri BuildUrl(string url)
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

        private JsonObject ParseResponse(HttpResponseMessage response)
        {
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
            return ExecuteRawAqlQuery(query.ToAql(), new JsonObject());
        }

        public List<JsonValue> ExecuteRawAqlQuery(string aql, JsonObject bindParams)
        {
            var results = new List<JsonValue>();
            
            // create cursor
            JsonObject response;
            try
            {
                response = Post("/_api/cursor", new JsonObject()
                    .Add("query", aql)
                    .Add("bindVars", bindParams)
                );
            }
            catch (HttpRequestException e) when (((
                e.InnerException as WebException)
                ?.Response as HttpWebResponse)
                ?.StatusCode == HttpStatusCode.NotFound
            )
            {
                // SOMETIMES! (non-deterministically) HttpClient throws
                // an exception on 404, even though it shouldn't.
                // Don't ask me why, just deal with it...
                throw new ArangoException(
                    404, 1203, "View or collection not found."
                );
            }

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

        public void CreateIndex(
            string collectionName,
            string indexType,
            string[] fields,
            JsonObject otherProperties = null
        )
        {
            JsonObject payload = new JsonObject();

            foreach (var pair in otherProperties ?? new JsonObject())
                payload.Add(pair.Key, pair.Value);

            payload.Add("type", indexType);
            payload.Add(
                "fields",
                new JsonArray(
                    fields
                        .Select(s => new JsonValue(s))
                        .ToArray()
                )
            );
            
            Post(
                "/_api/index?collection=" + WebUtility.UrlEncode(collectionName),
                payload
            );
        }
    }
}