using System;
using System.Collections.Generic;
using System.Reflection;
using Unisave.Exceptions;

namespace Unisave.Foundation
{
    /// <summary>
    /// An IoC service container
    /// </summary>
    public class Container
    {
        /// <summary>
        /// Type to closure that creates that type
        /// </summary>
        private readonly Dictionary<Type, Func<object>> simpleBinds
            = new Dictionary<Type, Func<object>>();
        
        /// <summary>
        /// Type to instance of that type
        /// </summary>
        private readonly Dictionary<Type, object> instanceBinds
            = new Dictionary<Type, object>();
        
        /// <summary>
        /// Type to closure that constructs a singleton instance
        /// </summary>
        private readonly Dictionary<Type, Func<object>> singletonBinds
            = new Dictionary<Type, Func<object>>();
        
        /// <summary>
        /// Keep track of instances that should not be disposed
        /// when removed from the container
        /// </summary>
        private readonly HashSet<object> instancesToNotDispose
            = new HashSet<object>();

        /// <summary>
        /// Resolves type from the container
        /// </summary>
        public T Resolve<T>()
        {
            return (T) Resolve(typeof(T));
        }

        /// <summary>
        /// Resolves type from the container
        /// </summary>
        public object Resolve(Type type)
        {
            // === find an instance ===

            if (instanceBinds.ContainsKey(type))
                return instanceBinds[type];
            
            // === find a singleton ===

            if (singletonBinds.ContainsKey(type))
            {
                object instance = singletonBinds[type].Invoke();
                singletonBinds.Remove(type);
                instanceBinds[type] = instance;
                return instance;
            }
            
            // === find simple bind ===

            if (simpleBinds.ContainsKey(type))
                return simpleBinds[type].Invoke();
            
            // === build instance manually ===

            return BuildTypeInstance(type);
        }

        /// <summary>
        /// Tries to create an instance of given type
        /// </summary>
        private object BuildTypeInstance(Type type)
        {
            ConstructorInfo[] constructors = type.GetConstructors();

            if (constructors.Length == 0)
                throw new BindingResolutionException(
                    $"Type {type} has no constructors and so cannot" +
                    " be instantiated."
                );
            
            if (constructors.Length > 1)
                throw new BindingResolutionException(
                    $"Type {type} has multiple constructors and so cannot" +
                    " be instantiated."
                );

            ConstructorInfo constructor = constructors[0];

            ParameterInfo[] parameters = constructor.GetParameters();
            
            object[] parameterValues = new object[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
                parameterValues[i] = Resolve(parameters[i].ParameterType);

            return constructor.Invoke(parameterValues);
        }

        /// <summary>
        /// Register a factory that can create instance of given type
        /// </summary>
        public void Bind<T>(Func<Container, T> factory)
        {
            if (factory == null)
                throw new ArgumentNullException();
            
            Forget<T>();
            simpleBinds.Add(typeof(T), () => factory.Invoke(this));
        }
        
        /// <summary>
        /// Bind an interface to implementation
        /// </summary>
        public void Bind<TAbstract, TConcrete>() where TConcrete : TAbstract
        {
            Bind<TAbstract>((ctr) => ctr.Resolve<TConcrete>());
        }

        /// <summary>
        /// Register a factory that creates singleton instance
        /// </summary>
        public void Singleton<T>(Func<Container, T> factory)
        {
            if (factory == null)
                throw new ArgumentNullException();
            
            Forget<T>();
            singletonBinds.Add(typeof(T), () => factory.Invoke(this));
        }
        
        /// <summary>
        /// Bind an interface to implementation as singleton
        /// </summary>
        public void Singleton<TAbstract, TConcrete>() where TConcrete : TAbstract
        {
            Singleton<TAbstract>((ctr) => ctr.Resolve<TConcrete>());
        }

        /// <summary>
        /// Registers a service that implements interface T
        /// </summary>
        public void Instance<T>(T instance)
        {
            if (instance == null)
                throw new ArgumentNullException();
            
            Forget<T>();
            instanceBinds.Add(typeof(T), instance);
        }

        /// <summary>
        /// Forget a given type
        /// </summary>
        public void Forget<T>()
        {
            simpleBinds.Remove(typeof(T));
            singletonBinds.Remove(typeof(T));

            if (instanceBinds.ContainsKey(typeof(T)))
            {
                TryDisposeInstance(instanceBinds[typeof(T)]);
                instanceBinds.Remove(typeof(T));
            }
        }

        /// <summary>
        /// Tries to dispose a given instance that is held in the container
        /// </summary>
        private void TryDisposeInstance(object instance)
        {
            if (instance is IDisposable disposableInstance)
            {
                if (instancesToNotDispose.Contains(instance))
                    return;
                
                disposableInstance.Dispose();
            }
        }

        /// <summary>
        /// Remember a registered instance to be managed by someone else
        /// </summary>
        public void DontDisposeInstance(object instance)
        {
            if (instance == null)
                throw new ArgumentNullException();

            instancesToNotDispose.Add(instance);
        }

        /// <summary>
        /// Disposes all disposable services and clears the service container
        /// </summary>
        public virtual void Dispose()
        {
            simpleBinds.Clear();
            singletonBinds.Clear();
            
            foreach (var pair in instanceBinds)
                TryDisposeInstance(pair.Value);
            instanceBinds.Clear();
            
            instancesToNotDispose.Clear();
        }
    }
}