using System;

namespace Unisave.Foundation
{
    /// <summary>
    /// Abstracts away an IoC container implementation for the framework
    /// </summary>
    public interface IContainer : IDisposable
    {
        /// <summary>
        /// Creates and returns a new instance of a child container
        /// (such that if the container is missing a registration,
        /// its resolution will fall back onto the parent)
        /// </summary>
        /// <returns></returns>
        IContainer CreateChildContainer();
        
        #region "Register Transient"
        
        /// <summary>
        /// Add/replace a type to be used each time an abstract type is resolved.
        /// A new instance will be created with each resolution.
        /// </summary>
        /// <param name="abstractType"></param>
        /// <param name="concreteType"></param>
        void RegisterTransient(Type abstractType, Type concreteType);
        
        /// <summary>
        /// Add/replace a type to be used each time an abstract type is resolved.
        /// A new instance will be created with each resolution.
        /// </summary>
        /// <typeparam name="TAbstractType"></typeparam>
        /// <typeparam name="TConcreteType"></typeparam>
        void RegisterTransient<TAbstractType, TConcreteType>()
            where TAbstractType : class
            where TConcreteType : class, TAbstractType;

        /// <summary>
        /// Add/replace a factory function that constructs a given type.
        /// A new instance will be created with each resolution.
        /// </summary>
        /// <param name="registerType"></param>
        /// <param name="factory"></param>
        void RegisterTransient(
            Type registerType,
            Func<IContainer, object> factory
        );
        
        /// <summary>
        /// Add/replace a factory function that constructs a given type.
        /// A new instance will be created with each resolution.
        /// </summary>
        /// <param name="factory"></param>
        /// <typeparam name="TRegisterType"></typeparam>
        void RegisterTransient<TRegisterType>(
            Func<IContainer, TRegisterType> factory
        ) where TRegisterType : class;
        
        #endregion
        
        #region "Register Singleton"

        /// <summary>
        /// Make a specific type be resolved as a singleton, not as transient
        /// </summary>
        /// <param name="registerType"></param>
        void RegisterSingleton(Type registerType);

        /// <summary>
        /// Make a specific type be resolved as a singleton, not as transient
        /// </summary>
        /// <typeparam name="TRegisterType"></typeparam>
        void RegisterSingleton<TRegisterType>()
            where TRegisterType : class;
        
        /// <summary>
        /// Add/replace a type to be used each time an abstract type is resolved.
        /// Only a single instance is created, during the first resolution.
        /// </summary>
        /// <param name="abstractType"></param>
        /// <param name="concreteType"></param>
        void RegisterSingleton(Type abstractType, Type concreteType);
        
        /// <summary>
        /// Add/replace a type to be used each time an abstract type is resolved.
        /// Only a single instance is created, during the first resolution.
        /// </summary>
        /// <typeparam name="TAbstractType"></typeparam>
        /// <typeparam name="TConcreteType"></typeparam>
        void RegisterSingleton<TAbstractType, TConcreteType>()
            where TAbstractType : class
            where TConcreteType : class, TAbstractType;

        /// <summary>
        /// Add/replace a factory function that constructs a given type.
        /// Only a single instance is created, during the first resolution.
        /// </summary>
        /// <param name="registerType"></param>
        /// <param name="factory"></param>
        void RegisterSingleton(
            Type registerType,
            Func<IContainer, object> factory
        );
        
        /// <summary>
        /// Add/replace a factory function that constructs a given type.
        /// Only a single instance is created, during the first resolution.
        /// </summary>
        /// <param name="factory"></param>
        /// <typeparam name="TRegisterType"></typeparam>
        void RegisterSingleton<TRegisterType>(
            Func<IContainer, TRegisterType> factory
        ) where TRegisterType : class;
        
        #endregion
        
        #region "Register Per-Request Singleton"
        
        /// <summary>
        /// Make a specific type be resolved as a per-request singleton
        /// when requested from a child container.
        /// </summary>
        /// <param name="registerType"></param>
        void RegisterPerRequestSingleton(Type registerType);
        
        /// <summary>
        /// Make a specific type be resolved as a per-request singleton
        /// when requested from a child container.
        /// </summary>
        /// <typeparam name="TRegisterType"></typeparam>
        void RegisterPerRequestSingleton<TRegisterType>()
            where TRegisterType : class;
        
        /// <summary>
        /// Add/replace a type to be used each time an abstract type is resolved.
        /// Only a single instance is created for each HTTP request processed.
        /// </summary>
        /// <param name="abstractType"></param>
        /// <param name="concreteType"></param>
        void RegisterPerRequestSingleton(Type abstractType, Type concreteType);
        
        /// <summary>
        /// Add/replace a type to be used each time an abstract type is resolved.
        /// Only a single instance is created for each HTTP request processed.
        /// </summary>
        /// <typeparam name="TAbstractType"></typeparam>
        /// <typeparam name="TConcreteType"></typeparam>
        void RegisterPerRequestSingleton<TAbstractType, TConcreteType>()
            where TAbstractType : class
            where TConcreteType : class, TAbstractType;
        
        /// <summary>
        /// Add/replace a factory function that constructs a given type.
        /// Only a single instance is created for each HTTP request processed.
        /// </summary>
        /// <param name="registerType"></param>
        /// <param name="factory"></param>
        void RegisterPerRequestSingleton(
            Type registerType,
            Func<IContainer, object> factory
        );
        
        /// <summary>
        /// Add/replace a factory function that constructs a given type.
        /// Only a single instance is created for each HTTP request processed.
        /// </summary>
        /// <param name="factory"></param>
        /// <typeparam name="TRegisterType"></typeparam>
        void RegisterPerRequestSingleton<TRegisterType>(
            Func<IContainer, TRegisterType> factory
        ) where TRegisterType : class;
        
        #endregion
        
        #region "Register Instance"

        /// <summary>
        /// Add/replace an instance to be returned for a given type.
        /// </summary>
        /// <param name="registerType"></param>
        /// <param name="instance"></param>
        void RegisterInstance(Type registerType, object instance);
        
        /// <summary>
        /// Add/replace an instance to be returned for a given type.
        /// </summary>
        /// <param name="registerType"></param>
        /// <param name="instance"></param>
        /// <param name="transferOwnership">
        /// If the ownership is transferred, the container is responsible for
        /// disposal of the instance.
        /// </param>
        void RegisterInstance(Type registerType, object instance, bool transferOwnership);
        
        /// <summary>
        /// Add/replace an instance to be returned for a given type.
        /// </summary>
        /// <param name="instance"></param>
        /// <typeparam name="TRegisterType"></typeparam>
        void RegisterInstance<TRegisterType>(TRegisterType instance)
            where TRegisterType : class;
        
        /// <summary>
        /// Add/replace an instance to be returned for a given type.
        /// </summary>
        /// <param name="instance"></param>
        /// <typeparam name="TRegisterType"></typeparam>
        /// <param name="transferOwnership">
        /// If the ownership is transferred, the container is responsible for
        /// disposal of the instance.
        /// </param>
        void RegisterInstance<TRegisterType>(TRegisterType instance, bool transferOwnership)
            where TRegisterType : class;
        
        #endregion
        
        #region "Unregister"

        /// <summary>
        /// Removes container registration for the given type
        /// </summary>
        /// <param name="registerType"></param>
        /// <returns>True if the type was actually registered</returns>
        bool Unregister(Type registerType);

        /// <summary>
        /// Removes container registration for the given type
        /// </summary>
        /// <typeparam name="TRegisterType"></typeparam>
        /// <returns>True if the type was actually registered</returns>
        bool Unregister<TRegisterType>() where TRegisterType : class;
        
        #endregion
        
        #region "Resolve"
        
        /// <summary>
        /// Attempts to resolve a type
        /// </summary>
        /// <param name="resolveType">The type to resolve</param>
        /// <returns>Instance of the resolved type</returns>
        object Resolve(Type resolveType);
        
        /// <summary>
        /// Attempts to resolve a type
        /// </summary>
        /// <typeparam name="TResolveType">Type to resolve</typeparam>
        /// <returns>Instance of the resolved type</returns>
        TResolveType Resolve<TResolveType>()
            where TResolveType : class;

        /// <summary>
        /// Returns true if the given type is explicitly registered
        /// </summary>
        /// <param name="resolveType"></param>
        /// <returns></returns>
        bool IsRegistered(Type resolveType);

        /// <summary>
        /// Returns true if the given type is explicitly registered
        /// </summary>
        /// <typeparam name="TResolveType"></typeparam>
        /// <returns></returns>
        bool IsRegistered<TResolveType>()
            where TResolveType : class;
        
        /// <summary>
        /// Returns true if the given type can be resolved
        /// (need not be registered if it's a plain type)
        /// </summary>
        /// <param name="resolveType"></param>
        /// <returns></returns>
        bool CanResolve(Type resolveType);

        /// <summary>
        /// Returns true if the given type can be resolved
        /// (need not be registered if it's a plain type)
        /// </summary>
        /// <typeparam name="TResolveType"></typeparam>
        /// <returns></returns>
        bool CanResolve<TResolveType>()
            where TResolveType : class;

        /// <summary>
        /// Tries to resolve a type from the container. If that fails,
        /// false is returned.
        /// </summary>
        /// <param name="resolveType"></param>
        /// <param name="instance"></param>
        /// <returns>True on success.</returns>
        bool TryResolve(Type resolveType, out object instance);

        /// <summary>
        /// Tries to resolve a type from the container. If that fails,
        /// false is returned.
        /// </summary>
        /// <param name="instance"></param>
        /// <typeparam name="TResolveType"></typeparam>
        /// <returns>True on success.</returns>
        bool TryResolve<TResolveType>(out TResolveType instance)
            where TResolveType : class;

        #endregion
    }
}