using Unisave.Contracts;
using Unisave.Exceptions;
using Unisave.Foundation;
using Unisave.Sessions;

namespace Unisave.Providers
{
    public class SessionServiceProvider : ServiceProvider
    {
        public SessionServiceProvider(Application app) : base(app) { }
        
        public override void Register()
        {
            App.Singleton<SessionIdRepository>(_ => new SessionIdRepository());
            
            App.Singleton<ISession>(app => {
                var env = app.Resolve<EnvStore>();
                
                string driver = env.GetString("SESSION_DRIVER", "arango");
                int lifetime = env.GetInt("SESSION_LIFETIME", 3600); // 1h

                switch (driver)
                {
                    case "arango":
                        return new SessionOverStorage(
                            new ArangoSessionStorage(
                                app.Resolve<IArango>(),
                                app.Resolve<ILog>()
                            ),
                            lifetime
                        );
                    
                    case "memory":
                        return new SessionOverStorage(
                            new InMemorySessionStorage(),
                            lifetime
                        );
                    
                    case "null":
                        return new SessionOverStorage(
                            null,
                            lifetime
                        );
                }
                
                throw new ConfigurationException(
                    $"Provided session driver '{driver}' is not valid."
                );
            });
        }
    }
}