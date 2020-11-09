using Unisave.Broadcasting;
using Unisave.Foundation;

namespace Unisave.Providers
{
    public class BroadcastingServiceProvider : ServiceProvider
    {
        public BroadcastingServiceProvider(Application app) : base(app) { }

        public override void Register()
        {
            // TODO: pull data from environment variables
            
            App.Singleton<IBroadcaster>(
                app => new UnisaveBroadcaster()
            );
        }
    }
}