using System;
using TinyIoC;
using Unisave.Bootstrapping;
using Unisave.Providers;
using Unisave.Runtime;

namespace Unisave.Foundation
{
    /// <summary>
    /// Contains the entire game backend application
    /// </summary>
    public class BackendApplication : IDisposable
    {
        /// <summary>
        /// All types defined inside the game backend that
        /// can be searched for appropriate user implementation
        /// </summary>
        public Type[] BackendTypes { get; }
        
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

        /// <summary>
        /// Creates a new game backend application instance
        /// </summary>
        /// <param name="backendTypes">All types in the game backend</param>
        /// <param name="envStore">Environment variables</param>
        public BackendApplication(
            Type[] backendTypes,
            EnvStore envStore,
            bool registerFrameworkServices = true
        )
        {
            BackendTypes = backendTypes;
            
            Services = new TinyIoCAdapter(new TinyIoCContainer());
            
            // register instances
            Services.RegisterInstance<BackendApplication>(this);
            Services.RegisterInstance<EnvStore>(envStore);
            
            if (registerFrameworkServices)
                RegisterServiceProviders();
            
            // run bootstrappers
            var engine = new BootstrappingEngine(BackendTypes);
            engine.Run();
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