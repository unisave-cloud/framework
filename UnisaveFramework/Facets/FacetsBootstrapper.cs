using System;
using System.IO;
using Owin;
using Unisave.Bootstrapping;
using Unisave.Foundation.Pipeline;

namespace Unisave.Facets
{
    public class FacetsBootstrapper : Bootstrapper
    {
        public const string FacetRequestKind = "Facet";
        
        public override int StageNumber => BootstrappingStage.Framework;

        public override Type[] RunBefore => new Type[] {
            typeof(UnisaveRequestMiddlewareBootstrapper)
        };

        private readonly UnisaveRequestMiddlewareBootstrapper router;

        public FacetsBootstrapper(UnisaveRequestMiddlewareBootstrapper router)
        {
            this.router = router;
        }
        
        public override void Main()
        {
            IAppBuilder owinAppBuilder = router.DefineBranch(FacetRequestKind);
            
            owinAppBuilder.Use(async (ctx, next) => {
                
                // TODO: parse the request and route it through the facet system
                
                ctx.Response.StatusCode = 200;
                ctx.Response.Headers["Content-Type"] = "text/plain";
                var sw = new StreamWriter(ctx.Response.Body);
                await sw.WriteAsync(
                    "Hello world from a facet handler!"
                );
                sw.Dispose();
            });
        }
    }
}