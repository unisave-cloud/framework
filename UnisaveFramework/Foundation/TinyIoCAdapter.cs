using System;
using TinyIoC;

namespace Unisave.Foundation
{
    /// <summary>
    /// Connects TinyIoC with the IContainer interface
    /// </summary>
    public class TinyIoCAdapter : IContainer
    {
        /// <summary>
        /// The inner TinyIoC container instance that does the heavy lifting
        /// </summary>
        private readonly TinyIoCContainer container;
        
        private bool disposed = false;

        public TinyIoCAdapter() : this(new TinyIoCContainer()) { }

        public TinyIoCAdapter(TinyIoCContainer container)
        {
            this.container = container;
            
            // register itself
            RegisterInstance<TinyIoCAdapter>(this, transferOwnership: false);
            RegisterInstance<IContainer>(this, transferOwnership: false);
        }

        /// <inheritdoc />
        public IContainer CreateChildContainer()
        {
            return new TinyIoCAdapter(
                container.GetChildContainer()
            );
        }
        
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
        
        #region "Register Per-Request Singleton"
        
        /// <inheritdoc />
        public void RegisterPerRequestSingleton(Type registerType)
            => RegisterPerRequestSingleton(registerType, registerType);
        
        /// <inheritdoc />
        public void RegisterPerRequestSingleton<TRegisterType>()
            where TRegisterType : class
            => RegisterPerRequestSingleton(
                typeof(TRegisterType),
                typeof(TRegisterType)
            );
        
        /// <inheritdoc />
        public void RegisterPerRequestSingleton(Type abstractType, Type concreteType)
        {
            RegisterPerRequestSingleton(abstractType, childContainer => {
                
                // get the underlying child TinyIoC instance
                TinyIoCContainer childTinyIoCContainer
                    = ((TinyIoCAdapter) childContainer).container;
                
                // Create the instance by the child container.
                // We don't use resolve as it would bubble up to the parent again.
                return childTinyIoCContainer.ConstructType(concreteType);
                
            });
        }

        /// <inheritdoc />
        public void RegisterPerRequestSingleton<TAbstractType, TConcreteType>()
            where TAbstractType : class
            where TConcreteType : class, TAbstractType
            => RegisterPerRequestSingleton(
                typeof(TAbstractType),
                typeof(TConcreteType)
            );

        /// <inheritdoc />
        public void RegisterPerRequestSingleton(
            Type registerType,
            Func<IContainer, object> factory
        )
        {
            container.Register(registerType, (_, __) => {
                
                /*
                 * This code is only called when a new instance is being created,
                 * because the resolution bubbled to the parent. After we register
                 * the new instance to the child, any subsequent resolutions
                 * will be handled by the child container.
                 *
                 * Therefore this method is only responsible for instance creation,
                 * never for retrieval.
                 */
                
                var context = RequestContext.Current;
        
                if (context == null)
                    throw new Exception(
                        "Per request singleton cannot be resolved from global context."
                    );
                
                // Create the instance by the child container.
                // We use the provided factory function.
                object instance = factory.Invoke(context.Services);
                
                // any future resolutions will use this instance and the child
                // will handle the instance lifetime and disposal
                context.Services.RegisterInstance(
                    registerType,
                    instance
                );
                
                return instance;
            });
        }

        /// <inheritdoc />
        public void RegisterPerRequestSingleton<TRegisterType>(
            Func<IContainer, TRegisterType> factory
        ) where TRegisterType : class
            => RegisterPerRequestSingleton(typeof(TRegisterType), factory);
        
        #endregion
        
        #region "Register Instance"

        /// <inheritdoc />
        public void RegisterInstance(Type registerType, object instance)
            => RegisterInstance(registerType, instance, transferOwnership: true);

        /// <inheritdoc />
        public void RegisterInstance(Type registerType, object instance, bool transferOwnership)
        {
            if (transferOwnership)
            {
                // by default, TinyIoC claims ownership
                container.Register(registerType, instance);
            }
            else
            {
                // for transient delegate-constructed instances
                // TinyIoC does not check IDisposable and so technically
                // does not claim the instance's ownership
                container.Register(registerType, (_, __) => instance);
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