using System;
using System.Threading.Tasks;
using Unisave.Foundation;

namespace Unisave.Bootstrapping
{
    /// <summary>
    /// Provides a base implementation of a Unisave bootstrapper,
    /// that implements the <see cref="IBootstrapper"/> interface.
    /// It has an asynchronous Main method.
    /// </summary>
    public abstract class AsyncBootstrapper : IBootstrapper
    {
        /// <inheritdoc cref="IBootstrapper.Services"/>
        public IContainer Services { set; get; }

        /// <inheritdoc cref="IBootstrapper.StageNumber"/>
        public virtual int StageNumber => BootstrappingStage.Default;

        /// <inheritdoc cref="IBootstrapper.RunAfter"/>
        public virtual Type[] RunAfter => Type.EmptyTypes;
        
        /// <inheritdoc cref="IBootstrapper.RunBefore"/>
        public virtual Type[] RunBefore => Type.EmptyTypes;

        /// <inheritdoc cref="IBootstrapper.Main"/>
        public abstract Task Main();

        void IBootstrapper.Main()
        {
            // do nothing
        }
        
        Task IBootstrapper.MainAsync()
        {
            // forward to the abstract method
            return Main();
        }
    }
}