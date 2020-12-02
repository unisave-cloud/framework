using Unisave.Broadcasting;
using Unisave.Facades;
using Unisave.Foundation;
using Unisave.HttpClient;
using Unisave.Sessions;

namespace Unisave.Providers
{
    public class BroadcastingServiceProvider : ServiceProvider
    {
        public BroadcastingServiceProvider(Application app) : base(app) { }

        public override void Register()
        {
            App.Singleton<IBroadcaster>(app => new UnisaveBroadcaster(
                app.Resolve<ServerSessionIdRepository>(),
                app.Resolve<Factory>(),
                Env.GetString("BROADCASTING_SERVER_URL"),
                Env.GetString("BROADCASTING_KEY"),
                Env.GetString("UNISAVE_ENVIRONMENT_ID")
            ));
        }
    }
}