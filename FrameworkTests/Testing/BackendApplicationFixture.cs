using System;
using System.Linq;
using NUnit.Framework;
using Unisave;
using Unisave.Foundation;

namespace FrameworkTests.Testing
{
    /// <summary>
    /// Base test suite class that lets you test a complete backend application,
    /// as if it was invoked by an OWIN host/server.
    /// </summary>
    public abstract class BackendApplicationFixture
    {
        /// <summary>
        /// Instance of the backend application that is created and teared down
        /// with each individual test.
        /// </summary>
        protected BackendApplication app;
        
        /// <summary>
        /// You should call <see cref="CreateApplication"/> method with the
        /// list of backend types to use here.
        /// </summary>
        [SetUp]
        public abstract void SetUpBackendApplication();

        /// <summary>
        /// Call this to initialize the backend application.
        /// The application is initialized to run in an in-memory setup.
        /// </summary>
        /// <param name="additionalTypes">
        /// Backend types to load, excluding the frameworks types.
        /// </param>
        /// <param name="overrideEnvVariables">
        /// You can provide a callback that modified environment variables
        /// as necessary.
        /// </param>
        protected virtual void CreateApplication(
            Type[] additionalTypes,
            Action<EnvStore> overrideEnvVariables = null
        )
        {
            var envStore = new EnvStore {
                ["ARANGO_DRIVER"] = "memory",
                ["SESSION_DRIVER"] = "memory"
            };
            
            overrideEnvVariables?.Invoke(envStore);

            app = BackendApplication.Start(
                typeof(FrameworkMeta).Assembly.GetTypes()
                    .Concat(additionalTypes).ToArray(),
                envStore
            );
        }

        /// <summary>
        /// Just calls <see cref="DisposeApplication"/> automatically.
        /// </summary>
        [TearDown]
        public virtual void TearDownBackendApplication()
        {
            DisposeApplication();
        }

        /// <summary>
        /// Override this to modify the behavior of app disposal.
        /// </summary>
        protected virtual void DisposeApplication()
        {
            app?.Dispose();
            app = null;
        }
    }
}