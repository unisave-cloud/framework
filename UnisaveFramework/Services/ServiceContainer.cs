using System;
using System.Collections.Generic;
using Unisave.Exceptions;

namespace Unisave.Services
{
    /// <summary>
    /// Contains a set of services that can be used throughout the framework.
    /// This container does not solve the implementation of those services,
    /// that is hidden behind interfaces.
    ///
    /// There's a default container that has to be initialized during boot-up
    /// process that is used by all facades and other static service accessors.
    /// </summary>
    public class ServiceContainer : IDisposable
    {
        /// <summary>
        /// Default container that is used by all static service accessors,
        /// when there's no other option to get a service instance
        /// </summary>
        public static ServiceContainer Default { get; set; }
        
        /// <summary>
        /// Map for each service interface to a service implementation
        /// </summary>
        private readonly Dictionary<Type, object> services
            = new Dictionary<Type, object>();

        /// <summary>
        /// Tries to resolve a service from the container given an interface
        /// that the service implements and was registered for
        /// </summary>
        /// <typeparam name="T">Service type</typeparam>
        /// <exception cref="UnisaveException">Service not found</exception>
        public T Resolve<T>()
        {
            if (TryResolve(out T service))
                return service;
            
            throw new UnisaveException(
                $"Service of type {typeof(T)} could not be found inside" +
                " the service container."
            );
        }

        /// <summary>
        /// Tries to resolve a service from the container given an interface
        /// that the service implements and was registered for
        /// </summary>
        /// <param name="service"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool TryResolve<T>(out T service)
        {
            bool gotten = services.TryGetValue(
                typeof(T),
                out object serviceObj
            );

            if (gotten)
            {
                service = (T) serviceObj;
                return true;
            }

            service = default(T);
            return false;
        }

        /// <summary>
        /// Returns true if a given service can be resolved
        /// </summary>
        public bool CanResolve<T>()
        {
            return TryResolve(out T _);
        }
        
        /// <summary>
        /// Registers a service that implements interface T
        /// </summary>
        public void Register<T>(object service)
        {
            if (service != null && !(service is T))
                throw new ArgumentException(
                    "Service does not implement the interface " + typeof(T)
                );
            
            services.Add(typeof(T), service);
        }

        /// <summary>
        /// Disposes all disposable services and clears the service container
        /// </summary>
        public void Dispose()
        {
            foreach (var pair in services)
            {
                if (pair.Value is IDisposable disposableService)
                    disposableService.Dispose();
            }
            
            services.Clear();
        }
    }
}