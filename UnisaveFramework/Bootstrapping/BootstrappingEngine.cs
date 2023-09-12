using System;
using System.Linq;

namespace Unisave.Bootstrapping
{
    /// <summary>
    /// This class is responsible for calling all bootstrappers properly.
    /// </summary>
    public class BootstrappingEngine
    {
        public Type[] BootstrapperTypes { get; }
        
        public BootstrappingEngine(Type[] searchedTypes)
        {
            BootstrapperTypes = searchedTypes.Where(type => {
                if (type.IsAbstract || type.IsValueType || type.IsInterface)
                    return false;
                
                if (type.IsGenericType || type.IsGenericTypeDefinition)
                    return false;
                
                if (!typeof(IBootstrapper).IsAssignableFrom(type))
                    return false;
                
                return true;
            })
                .OrderBy(type => type.FullName) // keep types ordered by name
                .ToArray();
        }

        public void Run()
        {
            // instantiate
            // TODO: use the service container, not the activator
            IBootstrapper[] bootstrapperInstances = BootstrapperTypes.Select(
                t => (IBootstrapper) Activator.CreateInstance(t)
            ).ToArray();
            
            // run
            // TODO: order them by dependencies
            foreach (IBootstrapper b in bootstrapperInstances)
                b.Main();
        }
    }
}