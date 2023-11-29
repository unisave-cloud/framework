using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Owin;
using NUnit.Framework;
using Unisave.Foundation;

namespace FrameworkTests.Testing.Foundation
{
    public static class BackendApplicationExtensions
    {
        /// <summary>
        /// Makes an HTTP request via the low-level OWIN HTTP interface
        /// </summary>
        /// <param name="app"></param>
        /// <param name="builder">Builds up the request to be sent</param>
        /// <returns>The response of the application</returns>
        public static async Task<IOwinResponse> HandleRequest(
            this BackendApplication app,
            Action<IOwinRequest> builder
        )
        {
            var ctx = new OwinContext();

            // prepare default request
            ctx.Request.Path = new PathString("/");

            // let the builder set up the request
            builder.Invoke(ctx.Request);

            // prepare response stream for writing
            var responseStream = new MemoryStream(10 * 1024); // 10 KB
            ctx.Response.Body = responseStream;
            
            // run the app delegate
            await app.Invoke(ctx);
            
            // check the HTTP response
            int receivedBytes = 0;
            if (ctx.Response.Headers.ContainsKey("Content-Length"))
            {
                receivedBytes = int.Parse(ctx.Response.Headers["Content-Length"]);
                Assert.GreaterOrEqual(receivedBytes, 0);
            }

            // prepare response stream for reading
            // (the writing stream was disposed which closes it for operations)
            ctx.Response.Body = new MemoryStream(
                responseStream.GetBuffer(), 0, receivedBytes, writable: false
            );
            
            return ctx.Response;
        }
    }
}