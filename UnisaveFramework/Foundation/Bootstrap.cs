using System;
using Unisave.Runtime;

namespace Unisave.Foundation
{
    public static class Bootstrap
    {
        /// <summary>
        /// Bootstraps a new application
        /// </summary>
        public static Application Boot(
            Type[] gameAssemblyTypes,
            Env env,
            SpecialValues specialValues
        )
        {
            var app = new Application(gameAssemblyTypes);
            
            // application can resolve itself
            app.Instance<Application>(app);
            app.DontDisposeInstance(app);
            
            // basic services, used even inside service providers
            app.Instance<Env>(env);
            app.Instance<SpecialValues>(specialValues);
            
            app.RegisterServiceProviders();

            return app;
        }
    }
}