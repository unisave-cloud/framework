using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Unisave.Bootstrapping;

namespace Unisave.Foundation.Pipeline
{
    public class UnisaveRequestMiddlewareBootstrapper : IBootstrapper
    {
        private readonly IAppBuilder owinAppBuilder;
        
        public UnisaveRequestMiddlewareBootstrapper(IAppBuilder owinAppBuilder)
        {
            this.owinAppBuilder = owinAppBuilder;
        }
        
        public void Main()
        {
            owinAppBuilder.MapWhen(IsUnisaveRequest, branch => {

                branch.Use(async (IOwinContext ctx, Func<Task> next) => {
                    ctx.Response.ContentType = "text/plain";
                    await ctx.Response.WriteAsync("Unisave Request!\n");
                });
                
            });
        }

        private static bool IsUnisaveRequest(IOwinContext ctx)
        {
            return ctx.Request.Headers.ContainsKey("X-Unisave-Request");
        }
    }
}