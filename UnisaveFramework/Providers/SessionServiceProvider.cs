using Unisave.Contracts;
using Unisave.Foundation;
using Unisave.Sessions;

namespace Unisave.Providers
{
    public class SessionServiceProvider : ServiceProvider
    {
        public SessionServiceProvider(Application app) : base(app) { }
        
        public override void Register()
        {
            App.Singleton<ISession>(
                app => new SandboxApiSession()
            );
        }
    }
}