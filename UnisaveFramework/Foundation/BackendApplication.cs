using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Builder;
using Owin;
using TinyIoC;
using Unisave.Bootstrapping;
using Unisave.Providers;

namespace Unisave.Foundation
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    
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
        /// The final OWIN AppFunc used to handle HTTP requests
        /// (set at the end of initialization)
        /// </summary>
        private AppFunc compiledOwinAppFunc;

        /// <summary>
        /// Has the application been already initialized
        /// </summary>
        private bool initialized = false;
        
        /// <summary>
        /// Has the application been already disposed
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// Creates a new game backend application instance
        /// </summary>
        /// <param name="backendTypes">
        /// All types in the game backend, including the UnisaveFramework
        /// </param>
        /// <param name="envStore">Environment variables</param>
        public BackendApplication(
            Type[] backendTypes,
            EnvStore envStore
        )
        {
            BackendTypes = backendTypes;
            
            Services = new TinyIoCAdapter(new TinyIoCContainer());
            
            // register instances
            Services.RegisterInstance<BackendApplication>(this);
            Services.RegisterInstance<EnvStore>(envStore);
            
            // initialize OWIN middleware stack
            // to be built up by the bootstrappers during initialization
            var owinAppBuilder = new AppBuilder();
            Services.RegisterInstance<IAppBuilder>(owinAppBuilder);
        }

        /// <summary>
        /// Creates and initializes a backend application in one method
        /// </summary>
        /// <returns>Ready to use backend application</returns>
        public static BackendApplication Start(
            Type[] backendTypes,
            EnvStore envStore
        )
        {
            var app = new BackendApplication(backendTypes, envStore);
            
            app.Initialize();

            return app;
        }

        /// <summary>
        /// Runs bootstrapping, preparing the instance to be used
        /// </summary>
        public void Initialize()
        {
            if (initialized)
            {
                throw new InvalidOperationException(
                    $"The {nameof(BackendApplication)} is already initialized."
                );
            }
            
            if (disposed)
            {
                throw new ObjectDisposedException(
                    $"The {nameof(BackendApplication)} is already disposed."
                );
            }

            RegisterServiceProviders();
            
            // run bootstrappers
            var engine = new BootstrappingEngine(Services, BackendTypes);
            engine.Run();
            
            // compile the OWIN AppFunc
            IAppBuilder owinApp = Services.Resolve<IAppBuilder>();
            compiledOwinAppFunc = owinApp.Build<AppFunc>();

            // initialization is complete
            initialized = true;
        }

        private void RegisterServiceProviders()
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

        /// <summary>
        /// Invoked for each HTTP request received
        /// </summary>
        /// <param name="context"></param>
        public Task Invoke(IOwinContext context)
        {
            if (!initialized)
            {
                throw new InvalidOperationException(
                    $"The {nameof(BackendApplication)} must be initialized before use."
                );
            }
            
            if (disposed)
            {
                throw new ObjectDisposedException(
                    $"The {nameof(BackendApplication)} is already disposed."
                );
            }
            
            // TODO: create request-scoped service container and attach it to the request
            
            // forward the request into the compiled OWIN AppFunc
            return compiledOwinAppFunc.Invoke(context.Environment);
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