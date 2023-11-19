using Unisave.Bootstrapping;
using Unisave.Contracts;
using Unisave.Foundation;
using Unisave.Sessions.Storage;

namespace Unisave.Sessions
{
    /// <summary>
    /// Bootstraps the session system
    /// and also acts as the session configuration object
    /// </summary>
    public class SessionBootstrapper : Bootstrapper
    {
        public override int StageNumber => BootstrappingStage.Framework;
        
        /// <summary>
        /// What driver is used by the session system
        /// (memory, arango, null)
        /// </summary>
        public string SessionDriver { get; }
        
        /// <summary>
        /// For how long should an inactive session be kept around
        /// </summary>
        public int SessionLifetimeSeconds { get; }

        public SessionBootstrapper(EnvStore env)
        {
            SessionDriver = env.GetString("SESSION_DRIVER", "arango");
            SessionLifetimeSeconds = env.GetInt("SESSION_LIFETIME", 3600); // 1h
        }
        
        public override void Main()
        {
            Services.RegisterPerRequestSingleton<ServerSessionIdRepository>();
            
            RegisterISessionInterface();
        }

        private void RegisterISessionInterface()
        {
            var env = Services.Resolve<EnvStore>();
            
            switch (SessionDriver)
            {
                case "arango":
                    RegisterArangoSession();
                    break;
                    
                case "memory":
                    RegisterInMemorySession();
                    break;
                    
                case "null":
                    RegisterNullSession();
                    break;
            }
        }

        private void RegisterInMemorySession()
        {
            Services.RegisterSingleton<ISessionStorage, InMemorySessionStorage>();
            
            Services.RegisterPerRequestSingleton<ISession>(
                container => new SessionFrontend(
                    container.Resolve<ISessionStorage>(),
                    SessionLifetimeSeconds
                )
            );
        }

        private void RegisterArangoSession()
        {
            // save/load to database with each request
            Services.RegisterPerRequestSingleton<ISessionStorage, ArangoSessionStorage>();
            
            Services.RegisterPerRequestSingleton<ISession>(
                container => new SessionFrontend(
                    container.Resolve<ISessionStorage>(),
                    SessionLifetimeSeconds
                )
            );
        }

        private void RegisterNullSession()
        {
            Services.RegisterSingleton<ISessionStorage, NullSessionStorage>();
            
            Services.RegisterPerRequestSingleton<ISession>(
                container => new SessionFrontend(
                    container.Resolve<ISessionStorage>(),
                    SessionLifetimeSeconds
                )
            );
        }
    }
}