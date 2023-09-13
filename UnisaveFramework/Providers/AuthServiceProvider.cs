using Unisave.Authentication;
using Unisave.Contracts;
using Unisave.Entities;
using Unisave.Foundation;

namespace Unisave.Providers
{
    public class AuthServiceProvider : ServiceProvider
    {
        public AuthServiceProvider(BackendApplication app) : base(app) { }

        public override void Register()
        {
            App.Services.RegisterSingleton<AuthenticationManager>(
                container => new AuthenticationManager(
                    container.Resolve<ISession>(),
                    container.Resolve<EntityManager>()
                )
            );
        }
    }
}