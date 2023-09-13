using System;
using TinyIoC;
using Unisave.Providers;

namespace Unisave.Foundation
{
    /// <summary>
    /// Contains the entire game backend application
    /// </summary>
    public class BackendApplication : IDisposable
    {
        /// <summary>
        /// All types defined inside the game assembly that
        /// can be searched for appropriate user implementation
        /// </summary>
        public Type[] GameAssemblyTypes { get; }
        
        /// <summary>
        /// IoC service container for the entire application
        /// </summary>
        public IContainer Services { get; }

        /// <summary>
        /// Loaded service providers
        /// </summary>
        private ServiceProvider[] providers;

        /// <summary>
        /// Has the application been already disposed
        /// </summary>
        private bool disposed = false;

        public BackendApplication(Type[] gameAssemblyTypes)
        {
            GameAssemblyTypes = gameAssemblyTypes;
            
            Services = new TinyIoCAdapter(new TinyIoCContainer());
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
                new HttpClientServiceProvider(this),
                new BroadcastingServiceProvider(this), 
            };
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            
            if (providers != null)
            {
                for (int i = providers.Length - 1; i >= 0; i--)
                    providers[i].TearDown();

                providers = null;
            }
            
            Services.Dispose();
        }
    }
}