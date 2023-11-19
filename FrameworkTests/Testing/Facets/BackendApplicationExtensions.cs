using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using LightJson;
using Microsoft.Owin;
using NUnit.Framework;
using Unisave.Foundation;

namespace FrameworkTests.Testing.Facets
{
    public static class BackendApplicationExtensions
    {
        /// <summary>
        /// Calls a facet method via the low-level OWIN HTTP interface
        /// </summary>
        /// <param name="app"></param>
        /// <param name="facetName"></param>
        /// <param name="methodName"></param>
        /// <param name="arguments"></param>
        /// <param name="requestCallback">
        /// Optional callback that lets you modify the OWIN request instance
        /// before it is sent.
        /// </param>
        /// <returns></returns>
        public static async Task<IOwinResponse> CallFacet(
            this BackendApplication app,
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
            // TODO: use some stream that can grow the buffer!
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
    }
}