using Unisave.Authentication;
using Unisave.Foundation;

namespace Unisave.Providers
{
    public class AuthServiceProvider : ServiceProvider
    {
        public AuthServiceProvider(Application app) : base(app) { }

        public override void Register()
        {
            App.Singleton<AuthenticationManager>(
                app => new AuthenticationManager()
            );
        }
    }
}