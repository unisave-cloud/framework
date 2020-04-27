using Unisave.Arango;
using Unisave.Arango.Emulation;
using Unisave.Contracts;
using Unisave.Exceptions;
using Unisave.Foundation;

namespace Unisave.Providers
{
    public class ArangoServiceProvider : ServiceProvider
    {
        public ArangoServiceProvider(Application app) : base(app) { }

        public override void Register()
        {
            App.Singleton<IArango>(app => {
                var env = app.Resolve<Env>();
                
                string driver = env.GetString("ARANGO_DRIVER", "http");

                switch (driver)
                {
                    case "http":
                        return new ArangoConnection(
                            env.GetString("ARANGO_BASE_URL"),
                            env.GetString("ARANGO_DATABASE"),
                            env.GetString("ARANGO_USERNAME"),
                            env.GetString("ARANGO_PASSWORD")
                        );
                    
                    case "memory":
                        return new ArangoInMemory();
                }
                
                throw new ConfigurationException(
                    $"Provided Arango driver '{driver}' is not valid."
                );
            });
        }
    }
}