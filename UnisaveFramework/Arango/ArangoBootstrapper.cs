using Unisave.Arango.Emulation;
using Unisave.Bootstrapping;
using Unisave.Contracts;
using Unisave.Exceptions;
using Unisave.Foundation;

namespace Unisave.Arango
{
    /// <summary>
    /// Bootstraps the connection to the ArangoDB database
    /// </summary>
    public class ArangoBootstrapper : Bootstrapper
    {
        public override int StageNumber => BootstrappingStage.Framework;
        
        public override void Main()
        {
            Services.RegisterSingleton<IArango>(container => {
                var env = container.Resolve<EnvStore>();
                
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