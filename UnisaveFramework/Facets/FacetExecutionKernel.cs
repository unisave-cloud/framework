using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Unisave.Foundation;
using Unisave.Sessions.Middleware;

namespace Unisave.Facets
{
    /// <summary>
    /// Handles the facet-call requests
    /// </summary>
    public class FacetExecutionKernel
    {
        private readonly IContainer services;
        
        public FacetExecutionKernel(IContainer services)
        {
            this.services = services;
        }
        
        /// <summary>
        /// Handle the facet call request
        /// </summary>
        public FacetResponse Handle(FacetRequest request)
        {
            MiddlewareAttribute[] globalMiddleware = {
                new MiddlewareAttribute(1, typeof(StartSession))
            };

            var response = FacetMiddleware.ExecuteMiddlewareStack(
                services,
                globalMiddleware,
                request,
                rq => {
                    
                    object[] deserializedArguments = Facet.DeserializeArguments(
                        request.Method,
                        request.Arguments
                    );

                    Facet facetInstance = Facet.CreateInstance(
                        request.FacetType,
                        services
                    );
                    
                    object returnedValue = null;
                    UnwrapTargetInvocationException(() => {
                        returnedValue = rq.Method.Invoke(
                            facetInstance,
                            deserializedArguments
                        );
                    });

                    return FacetResponse.CreateFrom(
                        returnedValue,
                        request.Method
                    );
                }
            );

            return response;
        }
        
        /// <summary>
        /// Invokes method via method info with transparent
        /// exception propagation
        /// </summary>
        private static void UnwrapTargetInvocationException(Action action)
        {
            try
            {
                action?.Invoke();
            }
            catch (TargetInvocationException e)
            {
                if (e.InnerException == null)
                    throw;
                    
                ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                throw;
            }
        }
    }
}