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
            SpecialValues specialValues
        )
        {
            var app = new Application(gameAssemblyTypes);
            
            app.Instance<Application>(app);
            app.DontDisposeInstance(app);
            
            app.Instance<SpecialValues>(specialValues);
            
            app.RegisterServiceProviders();

            return app;
        }
    }
}