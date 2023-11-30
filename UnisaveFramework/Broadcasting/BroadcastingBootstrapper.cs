using Unisave.Bootstrapping;
using Unisave.Foundation;
using Unisave.HttpClient;
using Unisave.Sessions;

namespace Unisave.Broadcasting
{
    /// <summary>
    /// Registers the broadcasting client service into the IoC container
    /// </summary>
    public class BroadcastingBootstrapper : Bootstrapper
    {
        public override int StageNumber => BootstrappingStage.Framework;
        
        public override void Main()
        {
            Services.RegisterPerRequestSingleton<IBroadcaster>(container => {
                var env = container.Resolve<EnvStore>();
                
                return new UnisaveBroadcaster(
                    container.Resolve<ServerSessionIdRepository>(),
                    container.Resolve<Factory>(),
                    env.GetString("BROADCASTING_SERVER_URL"),
                    env.GetString("BROADCASTING_KEY"),
                    env.GetString("UNISAVE_ENVIRONMENT_ID")
                );
            });
        }
    }
}