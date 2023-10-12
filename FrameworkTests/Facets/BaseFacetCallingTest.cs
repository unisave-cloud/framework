using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LightJson;
using Microsoft.Owin;
using NUnit.Framework;
using Unisave;
using Unisave.Facets;
using Unisave.Foundation;
using Unisave.Foundation.Pipeline;

namespace FrameworkTests.Facets
{
    public abstract class BaseFacetCallingTest
    {
        public BackendApplication app;
        
        protected virtual void CreateApplication(Type[] additionalTypes)
        {
            app = BackendApplication.Start(
                typeof(FrameworkMeta).Assembly.GetTypes()
                    .Concat(additionalTypes).ToArray(),
                new EnvStore()
            );
        }

        protected async Task<IOwinResponse> CallFacet(
            string facetName,
            string methodName,
            JsonArray arguments,
            string sessionId
        )
        {
            var ctx = new OwinContext();

            ctx.Request.Path = new PathString($"/{facetName}/{methodName}");
            ctx.Request.Headers[
                UnisaveRequestMiddleware.UnisaveRequestHeaderName
            ] = FacetsBootstrapper.FacetRequestKind;

            ctx.Response.Body = new MemoryStream();
            
            await app.Invoke(ctx);

            ctx.Response.Body.Seek(0, SeekOrigin.Begin);
            return ctx.Response;
        }
    }
}