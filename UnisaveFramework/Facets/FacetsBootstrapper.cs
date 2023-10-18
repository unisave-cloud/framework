using System;
using System.Text;
using System.Threading.Tasks;
using LightJson;
using Microsoft.Owin;
using Owin;
using Unisave.Bootstrapping;
using Unisave.Foundation.Pipeline;
using Unisave.Runtime.Kernels;
using Unisave.Serialization;

namespace Unisave.Facets
{
    public class FacetsBootstrapper : Bootstrapper
    {
        public override int StageNumber => BootstrappingStage.Framework;

        public override Type[] RunBefore => new Type[] {
            typeof(UnisaveRequestMiddlewareBootstrapper)
        };

        private readonly UnisaveRequestMiddlewareBootstrapper unisaveRequestRouter;

        public FacetsBootstrapper(UnisaveRequestMiddlewareBootstrapper unisaveRequestRouter)
        {
            this.unisaveRequestRouter = unisaveRequestRouter;
        }
        
        public override void Main()
        {
            unisaveRequestRouter
                .DefineBranch("Facet")
                .Run(HandleFacetRequest);
        }

        private async Task HandleFacetRequest(IOwinContext ctx)
        {
            // TODO: this should use request-scoped services, not global services!
            var requestServices = this.Services;

            JsonObject data;
            try
            {
                var kernel = requestServices.Resolve<FacetCallKernel>();
                var parameters =
                    await FacetCallKernel.MethodParameters.Parse(
                        ctx.Request);

                JsonValue returned = kernel.Handle(parameters);
                    
                data = new JsonObject()
                    .Add("status", "ok")
                    .Add("returned", returned);
                // TODO: logs
            }
            catch (Exception e)
            {
                data = new JsonObject()
                    .Add("status", "exception")
                    .Add("exception", Serializer.ToJson(e));
                // TODO: logs
            }

            await SendJson(ctx.Response, data);
        }

        private async Task SendJson(IOwinResponse response, JsonValue json)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(json.ToString());
            
            response.StatusCode = 200;
            response.Headers["Content-Type"] = "application/json";
            response.Headers["Content-Length"] = bytes.Length.ToString();
            
            await response.Body.WriteAsync(bytes, 0, bytes.Length);
            response.Body.Close();
        }
    }
}