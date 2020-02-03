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
            App.Singleton<ISession>(app => {
                // TODO: pull this from configuration
                string driver = "sandbox";
                int lifetime = 3600; // 1h

                switch (driver)
                {
                    case "sandbox":
                        return new SessionOverStorage(
                            new SandboxApiSessionStorage(),
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