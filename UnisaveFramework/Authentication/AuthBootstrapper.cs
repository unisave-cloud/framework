using Unisave.Bootstrapping;
using Unisave.Contracts;

namespace Unisave.Authentication
{
    /// <summary>
    /// Registers the authentication manager singleton
    /// </summary>
    public class AuthBootstrapper : Bootstrapper
    {
        public override int StageNumber => BootstrappingStage.Framework;
        
        public override void Main()
        {
            Services.RegisterPerRequestSingleton<AuthenticationManager>(
                container => new AuthenticationManager(
                    container.Resolve<ISession>(),
                    container.Resolve<IArango>()
                )
            );
        }
    }
}