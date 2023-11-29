using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin;
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
    /// Unisave Framework requires additional OWIN properties, listed at:
    /// https://unisave.cloud/docs/interfaces#custom-properties
    /// </summary>
    public class FrameworkStartup
    {
        public void Configuration(IAppBuilder app)
        {
            // get backend assemblies from properties via custom property
            Assembly[] gameAssemblies = (Assembly[]) app.Properties["unisave.GameAssemblies"];
            Type[] backendTypes = gameAssemblies.SelectMany(asm => asm.GetTypes()).ToArray();
            
            // load standard and unisave environment variables
            EnvStore env = PrepareEnvStore(app);
            
            // create new BackendApplication instance
            // and do its bootstrapping
            var backendApplication = BackendApplication.Start(backendTypes, env);
            
            // forward requests into the BackendApplication
            app.Run(backendApplication.Invoke);
            
            // register application tear down
            CancellationToken token = (CancellationToken)app.Properties["host.OnAppDisposing"];
            token.Register(() => {
                Console.WriteLine("Framework shutting down...");
                backendApplication.Dispose();
                Console.WriteLine("Done.");
            });
            
            // export the backend application
            app.Properties["unisave.BackendApplication"] = backendApplication;
        }

        private EnvStore PrepareEnvStore(IAppBuilder app)
        {
            var env = new EnvStore();

            // load standard variables
            foreach (DictionaryEntry de in Environment.GetEnvironmentVariables())
                env.Set(de.Key.ToString(), de.Value.ToString());
            
            // override with unisave variables
            if (app.Properties.TryGetValue("unisave.EnvironmentVariables", out var unisaveVars))
            {
                var unisaveVarsDictionary = (IDictionary<string, string>) unisaveVars;
                foreach (var pair in unisaveVarsDictionary)
                    env.Set(pair.Key, pair.Value);
            }

            return env;
        }
    }
}