using Unisave.Foundation;

namespace Unisave.Providers
{
    public abstract class ServiceProvider
    {
        protected Application App { get; }

        public ServiceProvider(Application app)
        {
            App = app;
        }

        /// <summary>
        /// Register services into the container
        /// </summary>
        public abstract void Register();

        /// <summary>
        /// Called during application teardown
        /// </summary>
        public virtual void TearDown()
        {
            // nothing, override this
        }
    }
}