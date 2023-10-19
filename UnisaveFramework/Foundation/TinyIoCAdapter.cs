using System;
using TinyIoC;

namespace Unisave.Foundation
{
    /// <summary>
    /// Connects TinyIoC with the IContainer interface
    /// </summary>
    public class TinyIoCAdapter : IContainer
    {
        private readonly TinyIoCContainer container;
        
        private bool disposed = false;

        public TinyIoCAdapter(TinyIoCContainer container)
        {
            this.container = container;
            
            // register itself
            RegisterInstance<TinyIoCAdapter>(this, transferOwnership: false);
            RegisterInstance<IContainer>(this, transferOwnership: false);
        }

        /// <inheritdoc />
        public IContainer CreateChildContainer()
            => new TinyIoCAdapter(container.GetChildContainer());
        
        #region "Register Transient"

        /// <inheritdoc />
        public void RegisterTransient(Type abstractType, Type concreteType)
            => container.Register(abstractType, concreteType).AsMultiInstance();

        /// <inheritdoc />
        public void RegisterTransient<TAbstractType, TConcreteType>()
            where TAbstractType : class
            where TConcreteType : class, TAbstractType
            => container.Register<TAbstractType, TConcreteType>().AsMultiInstance();

        /// <inheritdoc />
        public void RegisterTransient(
            Type registerType,
            Func<IContainer, object> factory
        ) => container.Register(registerType, (_, __) => factory.Invoke(this))
            .AsMultiInstance();

        /// <inheritdoc />
        public void RegisterTransient<TRegisterType>(
            Func<IContainer, TRegisterType> factory
        ) where TRegisterType : class
            => container.Register<TRegisterType>((_, __) => factory.Invoke(this))
                .AsMultiInstance();
        
        #endregion
        
        #region "Register Singleton"

        /// <inheritdoc />
        public void RegisterSingleton(Type registerType)
            => container.Register(registerType).AsSingleton();
        
        /// <inheritdoc />
        public void RegisterSingleton<TRegisterType>()
            where TRegisterType : class
            => container.Register<TRegisterType>().AsSingleton();
        
        /// <inheritdoc />
        public void RegisterSingleton(Type abstractType, Type concreteType)
            => container.Register(abstractType, concreteType).AsSingleton();

        /// <inheritdoc />
        public void RegisterSingleton<TAbstractType, TConcreteType>()
            where TAbstractType : class
            where TConcreteType : class, TAbstractType
            => container.Register<TAbstractType, TConcreteType>().AsSingleton();

        /// <inheritdoc />
        public void RegisterSingleton(
            Type registerType,
            Func<IContainer, object> factory
        ) => container.Register(registerType, (_, __) => factory.Invoke(this))
            .AsSingleton();

        /// <inheritdoc />
        public void RegisterSingleton<TRegisterType>(
            Func<IContainer, TRegisterType> factory
        ) where TRegisterType : class
            => container.Register<TRegisterType>((_, __) => factory.Invoke(this))
                .AsSingleton();
        
        #endregion
        
        #region "Register Instance"

        /// <inheritdoc />
        public void RegisterInstance(Type registerType, object instance)
            => RegisterInstance(registerType, instance, transferOwnership: true);

        /// <inheritdoc />
        public void RegisterInstance(Type registerType, object instance,
            bool transferOwnership)
        {
            if (transferOwnership)
            {
                // by default, TinyIoC claims ownership
                container.Register(registerType, instance);
            }
            else
            {
                // we need to register a dummy multi-instance first,
                // because that's one of the few instance factories that
                // can be converted to custom-lifetime factory later
                var options = container.Register(
                    registerType,
                    registerImplementation: instance.GetType()
                );
                TinyIoCContainer.RegisterOptions.ToCustomLifetimeManager(
                    options,
                    new ExternalInstanceLifetimeProvider(instance),
                    "external instance"
                );
            }
        }

        /// <inheritdoc />
        public void RegisterInstance<TRegisterType>(TRegisterType instance)
            where TRegisterType : class
            => RegisterInstance(typeof(TRegisterType), instance, transferOwnership: true);
        
        /// <inheritdoc />
        public void RegisterInstance<TRegisterType>(TRegisterType instance, bool transferOwnership)
            where TRegisterType : class
            => RegisterInstance(typeof(TRegisterType), instance, transferOwnership);
        
        #endregion
        
        #region "Unregister"

        /// <inheritdoc />
        public bool Unregister(Type registerType)
            => container.Unregister(registerType);

        /// <inheritdoc />
        public bool Unregister<TRegisterType>()
            where TRegisterType : class
            => container.Unregister<TRegisterType>();
        
        #endregion
        
        #region "Resolve"

        /// <inheritdoc />
        public object Resolve(Type resolveType)
            => container.Resolve(resolveType);

        /// <inheritdoc />
        public TResolveType Resolve<TResolveType>()
            where TResolveType : class
            => container.Resolve<TResolveType>();

        /// <inheritdoc />
        public bool IsRegistered(Type resolveType)
            => container.CanResolve(
                resolveType,
                ResolveOptions.FailUnregisteredAndNameNotFound // be strict
            );
        
        /// <inheritdoc />
        public bool IsRegistered<TResolveType>()
            where TResolveType : class
            => container.CanResolve<TResolveType>(
                ResolveOptions.FailUnregisteredAndNameNotFound // be strict
            );
        
        /// <inheritdoc />
        public bool CanResolve(Type resolveType)
            => container.CanResolve(resolveType);

        /// <inheritdoc />
        public bool CanResolve<TResolveType>()
            where TResolveType : class
            => container.CanResolve<TResolveType>();

        /// <inheritdoc />
        public bool TryResolve(Type resolveType, out object instance)
            => container.TryResolve(resolveType, out instance);

        /// <inheritdoc />
        public bool TryResolve<TResolveType>(out TResolveType instance)
            where TResolveType : class
            => container.TryResolve<TResolveType>(out instance);
        
        #endregion
        
        public void Dispose()
        {
            if (disposed)
                return;
            
            disposed = true;
            
            container.Dispose();
        }
    }
}