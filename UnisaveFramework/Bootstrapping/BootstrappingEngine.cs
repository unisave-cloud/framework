using System;
using System.Collections.Generic;
using System.Linq;
using Unisave.Foundation;

namespace Unisave.Bootstrapping
{
    /// <summary>
    /// This class is responsible for calling all bootstrappers properly.
    /// </summary>
    public class BootstrappingEngine
    {
        /// <summary>
        /// Container used for bootstrapper instantiation
        /// </summary>
        private readonly IContainer container;
        
        /// <summary>
        /// Final list of bootstrapper types that are instantiated
        /// </summary>
        public Type[] BootstrapperTypes { get; }
        
        /// <summary>
        /// Constructs new bootstrapping engine
        /// </summary>
        /// <param name="container">
        /// Container used for bootstrapper singleton registration and resolution
        /// </param>
        /// <param name="typesToSearch">
        /// Types to go through to find the bootstrapper classes
        /// </param>
        public BootstrappingEngine(
            IContainer container,
            IEnumerable<Type> typesToSearch
        )
        {
            this.container = container;
            
            BootstrapperTypes = typesToSearch.Where(type => {
                if (type.IsAbstract || type.IsValueType || type.IsInterface)
                    return false;
                
                if (type.IsGenericType || type.IsGenericTypeDefinition)
                    return false;
                
                if (!typeof(IBootstrapper).IsAssignableFrom(type))
                    return false;
                
                return true;
            })
                .Distinct() // remove duplicates
                .OrderBy(type => type.FullName) // keep types ordered by name
                .ToArray();
        }

        public void Run()
        {
            // register in the container as singletons
            foreach (Type t in BootstrapperTypes)
                container.RegisterSingleton(t);
            
            // instantiate via the container
            IBootstrapper[] bootstrapperInstances = BootstrapperTypes.Select(
                t => (IBootstrapper) container.Resolve(t)
            ).ToArray();
            
            // run
            // TODO: order them by dependencies and stages
            foreach (IBootstrapper b in bootstrapperInstances)
            {
                Console.WriteLine($"Running bootstrapper: {b.GetType().FullName} ...");
                b.Main();
            }
            Console.WriteLine("Bootstrapping done.");
        }
    }
}