using Unisave.Contracts;
using Unisave.Entities;
using Unisave.Foundation;

namespace Unisave.Providers
{
    public class EntityServiceProvider : ServiceProvider
    {
        public EntityServiceProvider(BackendApplication app) : base(app) { }

        public override void Register()
        {
            App.Services.RegisterSingleton<EntityManager>(
                container => new EntityManager(
                    container.Resolve<IArango>(),
                    container.Resolve<ILog>()
                )
            );
        }
    }
}