using System;
using System.Threading.Tasks;
using Unisave.Foundation;

namespace Unisave.Bootstrapping
{
    /// <summary>
    /// A bootstrapper is a class that is instantiated and run during
    /// Unisave Framework startup. It is analogous to the "Program.Main()"
    /// method, but is designed to be modular. Each backend module can have
    /// its own bootstrapper that registers services into the service container
    /// and prepares the module according to the provided configuration.
    /// Bootstrapper dependencies should be injected via its constructor.
    ///
    /// To create a bootstrapper, inherit from the <see cref="Bootstrapper"/>
    /// or <see cref="AsyncBootstrapper"/> classes instead of this interface.
    /// </summary>
    public interface IBootstrapper
    {
        /// <summary>
        /// Service container set by the bootstrapping framework right
        /// after constructions. Use this to resolve or register services.
        /// </summary>
        IContainer Services { get; set; }
        
        /// <summary>
        /// Bootstrappers are run in stages from the lowest number to the highest,
        /// this property should return the desired stage number
        /// </summary>
        int StageNumber { get; }
        
        /// <summary>
        /// Which bootstrappers should this bootstrapper run after
        /// </summary>
        Type[] RunAfter { get; }
        
        /// <summary>
        /// Before which bootstrappers should this bootstrapper run
        /// </summary>
        Type[] RunBefore { get; }
        
        /// <summary>
        /// This is like the "Program.Main()" method in console applications,
        /// it is executed when the bootstrapper is meant to run.
        /// </summary>
        void Main();

        /// <summary>
        /// An asynchronous variant of the Main method. Runs immediately after
        /// the synchronous one if you choose to use both. Better inherit from the
        /// <see cref="AsyncBootstrapper"/> class to make an async bootstrapper.
        /// </summary>
        Task MainAsync();
    }
}