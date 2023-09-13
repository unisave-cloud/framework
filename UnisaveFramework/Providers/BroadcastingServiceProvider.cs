using Unisave.Broadcasting;
using Unisave.Facades;
using Unisave.Foundation;
using Unisave.HttpClient;
using Unisave.Sessions;

namespace Unisave.Providers
{
    public class BroadcastingServiceProvider : ServiceProvider
    {
        public BroadcastingServiceProvider(BackendApplication app) : base(app) { }

        public override void Register()
        {
            App.Services.RegisterSingleton<IBroadcaster>(
                container => new UnisaveBroadcaster(
                    container.Resolve<ServerSessionIdRepository>(),
                    container.Resolve<Factory>(),
                    Env.GetString("BROADCASTING_SERVER_URL"),
                    Env.GetString("BROADCASTING_KEY"),
                    Env.GetString("UNISAVE_ENVIRONMENT_ID")
                )
            );
        }
    }
}