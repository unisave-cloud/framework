using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Builder;
using Microsoft.Owin.Security;
using Owin;
using TinyIoC;
using Unisave.Bootstrapping;
using Unisave.Providers;
using Unisave.Sessions;

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
        public BackendTypes BackendTypes { get; }
        
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
            BackendTypes = new BackendTypes(backendTypes);
            
            Services = new TinyIoCAdapter();
            
            // register instances
            Services.RegisterInstance<BackendApplication>(
                this, transferOwnership: false
            );
            Services.RegisterInstance<BackendTypes>(
                BackendTypes, transferOwnership: false
            );
            Services.RegisterInstance<EnvStore>(
                envStore, transferOwnership: false
            );
            
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
            var engine = Services.Resolve<BootstrappingEngine>();
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
        /// <param name="owinContext"></param>
        public async Task Invoke(IOwinContext owinContext)
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

            using (var requestContext = new RequestContext(Services, owinContext))
            {
                // TODO: get completely rid of SpecialValues, since they should
                // be scoped by the request, but are not anymore
                
                // TODO: make facades use RequestContext.Current
                
                // TODO catch exception and wrap in 500 HTTP response
                // (with more info than what would the host provide)
                // (figure out the output silencing for production on gateway,
                // probably via some HTTP response header and document it)
                // (or maybe do this in some exception handler?? or in addition?)
                // (for formatting custom exceptions?)
                
                // forward the request into the compiled OWIN AppFunc
                await compiledOwinAppFunc.Invoke(owinContext.Environment);
            }
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