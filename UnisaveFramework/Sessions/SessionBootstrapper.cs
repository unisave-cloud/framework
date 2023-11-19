using Unisave.Bootstrapping;
using Unisave.Contracts;
using Unisave.Foundation;
using Unisave.Sessions.Storage;

namespace Unisave.Sessions
{
    public class SessionBootstrapper : Bootstrapper
    {
        public override int StageNumber => BootstrappingStage.Framework;
        
        public override void Main()
        {
            Services.RegisterPerRequestSingleton<ServerSessionIdRepository>();
            
            RegisterISessionInterface();
        }

        private void RegisterISessionInterface()
        {
            var env = Services.Resolve<EnvStore>();
            
            string driver = env.GetString("SESSION_DRIVER", "arango");
            int lifetime = env.GetInt("SESSION_LIFETIME", 3600); // 1h
            
            switch (driver)
            {
                case "arango":
                    RegisterArangoSession(lifetime);
                    break;
                    
                case "memory":
                    RegisterInMemorySession(lifetime);
                    break;
                    
                case "null":
                    RegisterNullSession(lifetime);
                    break;
            }
        }

        private void RegisterInMemorySession(int lifetime)
        {
            Services.RegisterSingleton<ISessionStorage, InMemorySessionStorage>();
            
            Services.RegisterPerRequestSingleton<ISession>(
                container => new SessionFrontend(
                    container.Resolve<ISessionStorage>(),
                    lifetime
                )
            );
        }

        private void RegisterArangoSession(int lifetime)
        {
            // save/load to database with each request
            Services.RegisterPerRequestSingleton<ISessionStorage, ArangoSessionStorage>();
            
            Services.RegisterPerRequestSingleton<ISession>(
                container => new SessionFrontend(
                    container.Resolve<ISessionStorage>(),
                    lifetime
                )
            );
        }

        private void RegisterNullSession(int lifetime)
        {
            Services.RegisterSingleton<ISessionStorage, NullSessionStorage>();
            
            Services.RegisterPerRequestSingleton<ISession>(
                container => new SessionFrontend(
                    container.Resolve<ISessionStorage>(),
                    lifetime
                )
            );
        }
    }
}