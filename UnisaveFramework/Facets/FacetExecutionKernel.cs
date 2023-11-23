using System;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
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
        public async Task<FacetResponse> Handle(FacetRequest request)
        {
            MiddlewareAttribute[] globalMiddleware = {
                new MiddlewareAttribute(1, typeof(StartSession))
            };

            FacetResponse response = await FacetMiddleware.ExecuteMiddlewareStack(
                services,
                globalMiddleware,
                request,
                async (FacetRequest rq) => {
                    
                    object[] deserializedArguments = Facet.DeserializeArguments(
                        request.Method,
                        request.Arguments
                    );

                    Facet facetInstance = Facet.CreateInstance(
                        request.FacetType,
                        services
                    );
                    
                    object returnedValue = null;
                    Type returnType = rq.Method.ReturnType;
                    UnwrapTargetInvocationException(() => {
                        returnedValue = rq.Method.Invoke(
                            facetInstance,
                            deserializedArguments
                        );
                    });

                    await AwaitIfTask(returnedValue);

                    UnwrapIfTask(ref returnType, ref returnedValue);

                    return FacetResponse.CreateFrom(
                        returnedValue,
                        returnType
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

        private static async Task AwaitIfTask(object returnedValue)
        {
            if (returnedValue is Task task)
                await task;
        }

        private static void UnwrapIfTask(
            ref Type returnType,
            ref object returnedValue
        )
        {   
            // void tasks
            if (returnType == typeof(Task))
            {
                returnType = typeof(object);
                returnedValue = null;
                return;
            }
            
            // returning (generic) tasks
            if (returnType.IsGenericType
                && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            {
                // unwrap the return type
                returnType = returnType.GetGenericArguments()[0];
                
                // pass null along
                // (should not happen, because it should fail earlier on await)
                if (returnedValue == null)
                    return;

                // unwrap the value
                PropertyInfo pi = returnedValue.GetType().GetProperty("Result");
                returnedValue = pi.GetValue(returnedValue);
                return;
            }
            
            // else do nothing
        }
    }
}