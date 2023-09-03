using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup("UnisaveFramework", typeof(Unisave.FrameworkStartup))]

namespace Unisave
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    
    /// <summary>
    /// This class is found by OWIN loader as the entrypoint to the
    /// game backend application. This is where everything beings.
    ///
    /// Unisave Framework requires addition OWIN properties:
    ///
    /// "unisave.GameAssemblies"
    ///     of type System.Reflection.Assembly[]
    ///     Contains all the game backend assemblies that should be used
    ///     to look up Bootstrappers, Facets, Entities, etc.
    /// </summary>
    public class FrameworkStartup
    {
        public void Configuration(IAppBuilder app)
        {
            // build the middleware stack
            app.Use(async (IOwinContext ctx, Func<Task> next) => {
                ctx.Response.ContentType = "text/plain";
                await ctx.Response.WriteAsync("Hello from the framework Startup!\n");
            });
            
            // get backend assemblies from properties via custom property
            Assembly[] gameAssemblies = (Assembly[]) app.Properties["unisave.GameAssemblies"];
            
            // create new BackendApplication instance
            // and do its bootstrapping
            
            // register application tear down
            CancellationToken token = (CancellationToken)app.Properties["host.OnAppDisposing"];
            token.Register(() => {
                Console.WriteLine("Framework startup shutting down!");
            });
        }
    }
}