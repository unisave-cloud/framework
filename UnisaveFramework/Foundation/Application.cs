using System;
using Unisave.Providers;

namespace Unisave.Foundation
{
    /// <summary>
    /// Contains the entire application
    /// </summary>
    public class Application : Container, IDisposable
    {
        // TODO: move this to some Facet app access point
        public static Application Default { get; set; }

        /// <summary>
        /// All types defined inside the game assembly that
        /// can be searched for appropriate user implementation
        /// </summary>
        public Type[] GameAssemblyTypes { get; }

        public Application(Type[] gameAssemblyTypes)
        {
            GameAssemblyTypes = gameAssemblyTypes;
        }

        // TODO: replace this with something more configurable later
        public void RegisterServiceProviders()
        {
            ServiceProvider[] providers = {
                new SessionServiceProvider(this),
                new EntityServiceProvider(this),
                new ArangoServiceProvider(this),
                new AuthServiceProvider(this), 
            };
            
            foreach (var p in providers)
                p.Register();
        }
    }
}