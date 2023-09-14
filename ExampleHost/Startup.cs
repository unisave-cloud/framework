using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ExampleBackend;
using Microsoft.Owin;
using Owin;

namespace ExampleHost
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // resolve all the game backend assemblies
            // (these would be loaded from downloaded files during
            // backend code downloading)
            Assembly[] gameAssemblies = new Assembly[] {
                typeof(Unisave.FrameworkStartup).Assembly,
                typeof(Class1).Assembly
            };
            
            // populate game assemblies property
            app.Properties["unisave.GameAssemblies"] = gameAssemblies;
            
            // TODO: use the OWIN loader code to locate the proper startup
            // among all of the game assemblies
            Type startupType = typeof(Unisave.FrameworkStartup);
            
            // TODO: create the startup via OWIN code
            // and also call it that way
            var startup = new Unisave.FrameworkStartup();
            startup.Configuration(app);
        }
    }
}