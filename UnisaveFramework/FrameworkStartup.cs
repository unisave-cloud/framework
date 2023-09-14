using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Builder;
using Owin;
using Unisave.Foundation;

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
    ///     This should include the Unisave Framework assembly, as it may
    ///     in theory be replaced by a custom framework.
    /// </summary>
    public class FrameworkStartup
    {
        public void Configuration(IAppBuilder app)
        {
            // get backend assemblies from properties via custom property
            Assembly[] gameAssemblies = (Assembly[]) app.Properties["unisave.GameAssemblies"];
            Type[] backendTypes = gameAssemblies.SelectMany(asm => asm.GetTypes()).ToArray();
            
            // TODO: load unisave environment variables
            var env = new EnvStore();
            
            // create new BackendApplication instance
            // and do its bootstrapping
            var backendApplication = BackendApplication.Start(backendTypes, env);
            
            // forward requests into the BackendApplication
            app.Run(backendApplication.Invoke);
            
            // register application tear down
            CancellationToken token = (CancellationToken)app.Properties["host.OnAppDisposing"];
            token.Register(() => {
                Console.WriteLine("Framework startup shutting down!");
                backendApplication.Dispose();
            });
        }
    }
}