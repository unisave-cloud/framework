using Unisave.Bootstrapping;
using Unisave.Contracts;

namespace Unisave.Entities
{
    /// <summary>
    /// Registers the entity manager singleton
    /// that lies in the heart of the entity system
    /// </summary>
    public class EntitiesBootstrapper : Bootstrapper
    {
        public override int StageNumber => BootstrappingStage.Framework;

        public override void Main()
        {
            Services.RegisterPerRequestSingleton<EntityManager>(
                container => new EntityManager(
                    container.Resolve<IArango>(),
                    container.Resolve<ILog>()
                )
            );
        }
    }
}