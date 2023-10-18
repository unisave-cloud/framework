using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LightJson;
using Microsoft.Owin;
using NUnit.Framework;
using Unisave;
using Unisave.Foundation;
using Unisave.Serialization;

namespace FrameworkTests.Facets
{
    public abstract class BaseFacetCallingTest
    {
        public BackendApplication app;
        
        protected virtual void CreateApplication(Type[] additionalTypes)
        {
            var envStore = new EnvStore {
                ["ARANGO_DRIVER"] = "memory",
                ["SESSION_DRIVER"] = "memory"
            };

            app = BackendApplication.Start(
                typeof(FrameworkMeta).Assembly.GetTypes()
                    .Concat(additionalTypes).ToArray(),
                envStore
            );
        }

        protected async Task<IOwinResponse> CallFacet(
            string facetName,
            string methodName,
            JsonArray arguments,
            Action<IOwinRequest> requestCallback = null
        )
        {
            var ctx = new OwinContext();

            // prepare request
            ctx.Request.Path = new PathString($"/{facetName}/{methodName}");
            ctx.Request.Headers["X-Unisave-Request"] = "Facet";
            ctx.Request.Headers["Content-Type"] = "application/json";

            JsonObject requestBody = new JsonObject() {
                ["arguments"] = arguments
            };
            byte[] requestBodyBytes = Encoding.UTF8.GetBytes(requestBody.ToString());
            ctx.Request.Body = new MemoryStream(requestBodyBytes, writable: false);
            
            if (requestCallback != null)
                requestCallback.Invoke(ctx.Request);

            // prepare response stream for writing
            byte[] responseBuffer = new byte[100 * 1024];
            var responseStream = new MemoryStream(responseBuffer, writable: true);
            ctx.Response.Body = responseStream;
            
            // run the app delegate
            await app.Invoke(ctx);
            
            // check the HTTP response
            Assert.AreEqual(200, ctx.Response.StatusCode);
            Assert.AreEqual("application/json", ctx.Response.Headers["Content-Type"]);
            Assert.IsTrue(ctx.Response.Headers.ContainsKey("Content-Length"));
            int receivedBytes = int.Parse(ctx.Response.Headers["Content-Length"]);
            Assert.GreaterOrEqual(receivedBytes, 0);

            // prepare response stream for reading
            ctx.Response.Body = new MemoryStream(
                responseBuffer, 0, receivedBytes, writable: false
            );
            
            return ctx.Response;
        }

        protected virtual async Task<JsonObject> GetResponseBody(IOwinResponse response)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            string json = await new StreamReader(response.Body).ReadToEndAsync();
            return Serializer.FromJsonString<JsonObject>(json);
        }

        protected virtual async Task<T> GetReturnedValue<T>(IOwinResponse response)
        {
            JsonObject body = await GetResponseBody(response);
            Assert.AreEqual("ok", body["status"].AsString);
            return Serializer.FromJson<T>(body["returned"]);
        }
        
        protected virtual async Task<T> GetThrownException<T>(IOwinResponse response)
            where T : Exception
        {
            JsonObject body = await GetResponseBody(response);
            Assert.AreEqual("exception", body["status"].AsString);
            Assert.IsTrue(body.ContainsKey("isKnownException"));
            return Serializer.FromJson<T>(body["exception"]);
        }

        protected virtual async Task AssertOkVoidResponse(IOwinResponse response)
        {
            JsonObject body = await GetResponseBody(response);
            Assert.AreEqual("ok", body["status"].AsString);
            Assert.IsTrue(body["returned"].IsNull);
        }
    }
}