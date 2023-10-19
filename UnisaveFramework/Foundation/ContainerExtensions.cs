using System;

namespace Unisave.Foundation
{
    /// <summary>
    /// Additional <see cref="IContainer"/> methods that are implemented via
    /// the existing methods in the interface
    /// </summary>
    public static class ContainerExtensions
    {
        #region "Try Register Transient"

        /// <summary>
        /// Add a type (if not present) to be used each time an abstract type is resolved.
        /// A new instance will be created with each resolution.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="abstractType"></param>
        /// <param name="concreteType"></param>
        /// <returns>True if the type was registered.</returns>
        public static bool TryRegisterTransient(
            this IContainer container,
            Type abstractType,
            Type concreteType
        )
        {
            if (!container.IsRegistered(abstractType))
            {
                container.RegisterTransient(abstractType, concreteType);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add a type (if not present) to be used each time an abstract type is resolved.
        /// A new instance will be created with each resolution.
        /// </summary>
        /// <param name="container"></param>
        /// <typeparam name="TAbstractType"></typeparam>
        /// <typeparam name="TConcreteType"></typeparam>
        /// <returns>True if the type was registered.</returns>
        public static bool TryRegisterTransient<TAbstractType, TConcreteType>(
            this IContainer container
        )
            where TAbstractType : class
            where TConcreteType : class, TAbstractType
        {
            if (!container.IsRegistered<TAbstractType>())
            {
                container.RegisterTransient<TAbstractType, TConcreteType>();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add a factory function (if not present) that constructs a given type.
        /// A new instance will be created with each resolution.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="registerType"></param>
        /// <param name="factory"></param>
        /// <returns>True if the factory was registered.</returns>
        public static bool TryRegisterTransient(
            this IContainer container,
            Type registerType,
            Func<IContainer, object> factory
        )
        {
            if (!container.IsRegistered(registerType))
            {
                container.RegisterTransient(registerType, factory);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add a factory function (if not present) that constructs a given type.
        /// A new instance will be created with each resolution.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="factory"></param>
        /// <typeparam name="TRegisterType"></typeparam>
        /// <returns>True if the factory was registered.</returns>
        public static bool TryRegisterTransient<TRegisterType>(
            this IContainer container,
            Func<IContainer, TRegisterType> factory
        ) where TRegisterType : class
        {
            if (!container.IsRegistered<TRegisterType>())
            {
                container.RegisterTransient<TRegisterType>(factory);
                return true;
            }

            return false;
        }
        
        #endregion
        
        #region "Try Register Singleton"

        /// <summary>
        /// Make a specific type be resolved as a singleton, not as transient
        /// (if that type has not been registered already)
        /// </summary>
        /// <param name="container"></param>
        /// <param name="registerType"></param>
        /// <returns>True if the type was registered.</returns>
        public static bool TryRegisterSingleton(
            this IContainer container,
            Type registerType
        )
        {
            if (!container.IsRegistered(registerType))
            {
                container.RegisterSingleton(registerType);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Make a specific type be resolved as a singleton, not as transient
        /// (if that type has not been registered already)
        /// </summary>
        /// <param name="container"></param>
        /// <typeparam name="TRegisterType"></typeparam>
        /// <returns>True if the type was registered.</returns>
        public static bool TryRegisterSingleton<TRegisterType>(
            this IContainer container
        ) where TRegisterType : class
        {
            if (!container.IsRegistered<TRegisterType>())
            {
                container.RegisterSingleton<TRegisterType>();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add a type (if not present) to be used each time an abstract type is resolved.
        /// Only a single instance is created, during the first resolution.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="abstractType"></param>
        /// <param name="concreteType"></param>
        /// <returns>True if the type was registered.</returns>
        public static bool TryRegisterSingleton(
            this IContainer container,
            Type abstractType,
            Type concreteType
        )
        {
            if (!container.IsRegistered(abstractType))
            {
                container.RegisterSingleton(abstractType, concreteType);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add a type (if not present) to be used each time an abstract type is resolved.
        /// Only a single instance is created, during the first resolution.
        /// </summary>
        /// <param name="container"></param>
        /// <typeparam name="TAbstractType"></typeparam>
        /// <typeparam name="TConcreteType"></typeparam>
        /// <returns>True if the type was registered.</returns>
        public static bool TryRegisterSingleton<TAbstractType, TConcreteType>(
            this IContainer container
        )
            where TAbstractType : class
            where TConcreteType : class, TAbstractType
        {
            if (!container.IsRegistered<TAbstractType>())
            {
                container.RegisterSingleton<TAbstractType, TConcreteType>();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add a factory function (if not present) that constructs a given type.
        /// Only a single instance is created, during the first resolution.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="registerType"></param>
        /// <param name="factory"></param>
        /// <returns>True if the factory was registered.</returns>
        public static bool TryRegisterSingleton(
            this IContainer container,
            Type registerType,
            Func<IContainer, object> factory
        )
        {
            if (!container.IsRegistered(registerType))
            {
                container.RegisterSingleton(registerType, factory);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add a factory function (if not present) that constructs a given type.
        /// Only a single instance is created, during the first resolution.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="factory"></param>
        /// <typeparam name="TRegisterType"></typeparam>
        /// <returns>True if the factory was registered.</returns>
        public static bool TryRegisterSingleton<TRegisterType>(
            this IContainer container,
            Func<IContainer, TRegisterType> factory
        ) where TRegisterType : class
        {
            if (!container.IsRegistered<TRegisterType>())
            {
                container.RegisterSingleton<TRegisterType>(factory);
                return true;
            }

            return false;
        }
        
        #endregion
        
        #region "Try Register Instance"

        /// <summary>
        /// Add an instance (if not present) to be returned for a given type.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="registerType"></param>
        /// <param name="instance"></param>
        /// <returns>True if the instance was registered.</returns>
        public static bool TryRegisterInstance(
            this IContainer container,
            Type registerType,
            object instance
        )
        {
            if (!container.IsRegistered(registerType))
            {
                container.RegisterInstance(registerType, instance);
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Add an instance (if not present) to be returned for a given type.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="registerType"></param>
        /// <param name="instance"></param>
        /// <param name="transferOwnership"></param>
        /// <returns>True if the instance was registered.</returns>
        public static bool TryRegisterInstance(
            this IContainer container,
            Type registerType,
            object instance,
            bool transferOwnership
        )
        {
            if (!container.IsRegistered(registerType))
            {
                container.RegisterInstance(registerType, instance, transferOwnership);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add an instance (if not present) to be returned for a given type.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="instance"></param>
        /// <typeparam name="TRegisterType"></typeparam>
        /// <returns>True if the instance was registered.</returns>
        public static bool TryRegisterInstance<TRegisterType>(
            this IContainer container,
            TRegisterType instance
        ) where TRegisterType : class
        {
            if (!container.IsRegistered<TRegisterType>())
            {
                container.RegisterInstance<TRegisterType>(instance);
                return true;
            }

            return false;
        }
        
        /// <summary>
        /// Add an instance (if not present) to be returned for a given type.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="instance"></param>
        /// <typeparam name="TRegisterType"></typeparam>
        /// <param name="transferOwnership"></param>
        /// <returns>True if the instance was registered.</returns>
        public static bool TryRegisterInstance<TRegisterType>(
            this IContainer container,
            TRegisterType instance,
            bool transferOwnership
        ) where TRegisterType : class
        {
            if (!container.IsRegistered<TRegisterType>())
            {
                container.RegisterInstance<TRegisterType>(instance, transferOwnership);
                return true;
            }

            return false;
        }
        
        #endregion
    }
}