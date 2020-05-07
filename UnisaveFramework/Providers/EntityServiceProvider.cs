using Unisave.Contracts;
using Unisave.Entities;
using Unisave.Foundation;

namespace Unisave.Providers
{
    public class EntityServiceProvider : ServiceProvider
    {
        public EntityServiceProvider(Application app) : base(app) { }

        public override void Register()
        {
            App.Singleton<EntityManager>(
                app => new EntityManager(
                    app.Resolve<IArango>(),
                    app.Resolve<ILog>()
                )
            );
        }
    }
}