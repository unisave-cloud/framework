using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FrameworkTests.Testing;
using FrameworkTests.Testing.Foundation;
using LightJson;
using Microsoft.Owin;
using NUnit.Framework;
using Owin;
using Unisave.Bootstrapping;
using Unisave.Foundation;

namespace FrameworkTests.Foundation
{
    /// <summary>
    /// Tests that the <see cref="BackendApplication"/> can handle otherwise
    /// unhandled exceptions that arise inside the request handler. 
    /// </summary>
    [TestFixture]
    public class RootExceptionHandlingTest : BackendApplicationFixture
    {
        #region "Backend definition"
        
        public class MyBootstrapper : Bootstrapper
        {
            public static Action<IOwinResponse> requestHandler
                = (IOwinResponse response) => { };
            
            private readonly IAppBuilder owinAppBuilder;

            public MyBootstrapper(IAppBuilder owinAppBuilder)
            {
                this.owinAppBuilder = owinAppBuilder;
            }
            
            public override void Main()
            {
                owinAppBuilder.Use(HandleRequest);
            }
            
            private Task HandleRequest(IOwinContext ctx, Func<Task> next)
            {
                requestHandler.Invoke(ctx.Response);
                
                return Task.CompletedTask;
            }
        }
        
        #endregion
        
        public override void SetUpBackendApplication()
        {
            CreateApplication(new Type[] {
                typeof(MyBootstrapper)
            });
        }
        
        [Test]
        public async Task NoExceptionProducesOkResponse()
        {
            MyBootstrapper.requestHandler = (IOwinResponse r) => {
                // no exception
            };

            IOwinResponse response = await app.HandleRequest(request => {
                // some dummy empty request
                request.Headers["Foo"] = "Bar";
            });

            Assert.AreEqual(200, response.StatusCode);
        }

        [Test]
        public async Task ItCatchesExceptionAndProducesUnisaveErrorResponse()
        {
            MyBootstrapper.requestHandler = (IOwinResponse r) => {
                throw new Exception("Hello world!");
            };

            IOwinResponse response = await app.HandleRequest(request => {
                // some dummy empty request
                request.Headers["Foo"] = "Bar";
            });

            Assert.AreEqual(500, response.StatusCode);

            JsonObject body = await response.ReadJsonBody<JsonObject>();
            
            Assert.IsTrue(body.ContainsKey("exception"));
            
            Assert.AreEqual("Hello world!", body["exception"]["Message"].AsString);
            Assert.AreEqual("System.Exception", body["exception"]["ClassName"].AsString);
        }

        [Test]
        public async Task ItOnlyClosesStreamWhenHeadersAlreadySent()
        {
            MyBootstrapper.requestHandler = (IOwinResponse r) => {
                // send only a part of the response
                r.StatusCode = 201;
                r.Headers["Content-Type"] = "text/plain";
                r.Headers["Content-Length"] = "256";
                
                byte[] bytes = Encoding.UTF8.GetBytes("Hello");
                r.Body.Write(bytes, 0, bytes.Length);
                
                throw new Exception("Hello world!");
            };

            IOwinResponse response = await app.HandleRequest(request => {
                // some dummy empty request
                request.Headers["Foo"] = "Bar";
            });

            Assert.AreEqual(201, response.StatusCode);

            response.Body.Seek(0, SeekOrigin.Begin);
            string received = await new StreamReader(response.Body)
                .ReadToEndAsync();
            
            Assert.AreEqual("Hello\0\0\0\0\0", received.Substring(0, 10));
        }
    }
}