using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

namespace ExampleHost
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // TODO: use the ExampleBackend project via the Unisave Framework

            // custom middleware
            app.Use(async (IOwinContext ctx, Func<Task> next) => {

                ctx.Response.ContentType = "text/plain";
                await ctx.Response.WriteAsync("Hello world!\n");
                
            });
        }
    }
}