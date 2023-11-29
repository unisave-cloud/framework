using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using LightJson;
using Microsoft.Owin;
using Microsoft.Owin.Builder;
using Owin;
using Unisave.Bootstrapping;
using Unisave.Serialization;

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
            
            // run bootstrappers
            var engine = Services.Resolve<BootstrappingEngine>();
            engine.Run();
            
            // compile the OWIN AppFunc
            IAppBuilder owinApp = Services.Resolve<IAppBuilder>();
            compiledOwinAppFunc = owinApp.Build<AppFunc>();

            // initialization is complete
            initialized = true;
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
                try
                {
                    // forward the request into the compiled OWIN AppFunc
                    await compiledOwinAppFunc.Invoke(owinContext.Environment);
                }
                catch (Exception e)
                {
                    await HandleRootException(requestContext.OwinResponse, e);
                }
            }
        }

        private async Task HandleRootException(IOwinResponse response, Exception e)
        {
            // log the exception to output
            Console.WriteLine(e.ToString());

            // if headers are already sent, we cannot send the error response
            if (response.Body.Position != 0 || !response.Body.CanWrite)
                return;

            // send the "Unisave Error Response" body
            JsonObject json = new JsonObject() {
                ["exception"] = Serializer.ToJson(e)
            };
            
            byte[] bytes = Encoding.UTF8.GetBytes(json.ToString());
            
            response.StatusCode = 500;
            response.Headers["Content-Type"] = "application/json";
            response.Headers["Content-Length"] = bytes.Length.ToString();
            response.Headers["X-Unisave-Error-Response"] = "1.0";
            
            await response.Body.WriteAsync(bytes, 0, bytes.Length);
            response.Body.Close();
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            
            Services.Dispose();
        }
    }
}