using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using LightJson;
using Microsoft.Owin;
using Owin;
using Unisave.Bootstrapping;
using Unisave.Foundation;
using Unisave.Foundation.Pipeline;
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
        
        
        ////////////////////////////
        // Request-scoped methods //
        ////////////////////////////

        /// <summary>
        /// Called by the OWIN middleware pipeline whenever a facet request
        /// HTTP request is to be processed.
        /// </summary>
        private async Task HandleFacetRequest(IOwinContext ctx)
        {
            var requestServices = ctx.Get<IContainer>("unisave.RequestServices");
            
            JsonObject data;
            try
            {
                var kernel = requestServices.Resolve<FacetExecutionKernel>();
                var backendTypes = requestServices.Resolve<BackendTypes>();
                
                FacetRequest request = await ParseFacetRequest(
                    ctx.Request,
                    backendTypes
                );

                FacetResponse response = kernel.Handle(request);
                    
                data = new JsonObject()
                    .Add("status", "ok")
                    .Add("returned", response.ReturnedJson);
                // TODO: logs
            }
            catch (Exception e)
            {
                data = new JsonObject()
                    .Add("status", "exception")
                    .Add("exception", Serializer.ToJson(e))
                    .Add("isKnownException", false); // TODO: add the known-exception system
                // TODO: logs
            }

            await SendJson(ctx.Response, data);
        }

        private async Task<FacetRequest> ParseFacetRequest(
            IOwinRequest owinRequest,
            BackendTypes backendTypes
        )
        {
            string[] segments = owinRequest.Path.Value.Split('/');

            if (segments.Length != 3)
                throw new ArgumentException(
                    "Facet request HTTP path has invalid number of segments."
                );

            using (var sr = new StreamReader(owinRequest.Body, Encoding.UTF8))
            {
                string json = await sr.ReadToEndAsync();

                JsonObject body = Serializer.FromJsonString<JsonObject>(json);
                
                return FacetRequest.CreateFrom(
                    facetName: segments[1],
                    methodName: segments[2],
                    jsonArguments: body["arguments"].AsJsonArray,
                    backendTypes
                );
            }
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