using System;
using Unisave.Bootstrapping;
using Unisave.Runtime;

namespace Unisave.Foundation
{
    /// <summary>
    /// This is a legacy application boot class and has nothing to do with
    /// the bootstrapping system. This class will not be present in the new
    /// OWIN framework API.
    /// </summary>
    public static class Bootstrap
    {
        /// <summary>
        /// Bootstraps a new application
        /// </summary>
        public static Application Boot(
            Type[] gameAssemblyTypes,
            EnvStore envStore,
            SpecialValues specialValues
        )
        {
            var app = new Application(gameAssemblyTypes);
            
            // application can be resolved
            app.Services.RegisterInstance<Application>(app);
            
            // basic services, used even inside service providers
            app.Services.RegisterInstance<EnvStore>(envStore);
            app.Services.RegisterInstance<SpecialValues>(specialValues);
            
            app.RegisterServiceProviders();
            
            // bootstrapping
            var engine = new BootstrappingEngine(gameAssemblyTypes);
            engine.Run();

            return app;
        }
    }
}