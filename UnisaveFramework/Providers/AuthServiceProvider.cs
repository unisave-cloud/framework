using Unisave.Authentication;
using Unisave.Contracts;
using Unisave.Entities;
using Unisave.Foundation;

namespace Unisave.Providers
{
    public class AuthServiceProvider : ServiceProvider
    {
        public AuthServiceProvider(Application app) : base(app) { }

        public override void Register()
        {
            App.Singleton<AuthenticationManager>(
                app => new AuthenticationManager(
                    app.Resolve<ISession>(),
                    app.Resolve<EntityManager>()
                )
            );
        }
    }
}