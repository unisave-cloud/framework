using System;
using Unisave.Providers;

namespace Unisave.Foundation
{
    /// <summary>
    /// Contains the entire application
    /// </summary>
    public class Application : Container, IDisposable
    {
        /// <summary>
        /// All types defined inside the game assembly that
        /// can be searched for appropriate user implementation
        /// </summary>
        public Type[] GameAssemblyTypes { get; }

        /// <summary>
        /// Loaded service providers
        /// </summary>
        private ServiceProvider[] providers;

        public Application(Type[] gameAssemblyTypes)
        {
            GameAssemblyTypes = gameAssemblyTypes;
        }

        public void RegisterServiceProviders()
        {
            LoadServiceProviders();
            
            foreach (var p in providers)
                p.Register();
        }

        // TODO: replace this with something more configurable later
        private void LoadServiceProviders()
        {
            providers = new ServiceProvider[] {
                new LogServiceProvider(this),
                new SessionServiceProvider(this),
                new EntityServiceProvider(this),
                new ArangoServiceProvider(this),
                new AuthServiceProvider(this),
                new HttpClientServiceProvider(this) 
            };
        }

        public override void Dispose()
        {
            if (providers != null)
            {
                for (int i = providers.Length - 1; i >= 0; i--)
                    providers[i].TearDown();

                providers = null;
            }
            
            base.Dispose();
        }
    }
}