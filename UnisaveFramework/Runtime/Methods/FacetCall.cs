using System;
using LightJson;
using Unisave.Contracts;
using Unisave.Facets;
using Unisave.Services;
using Unisave.Sessions;

namespace Unisave.Runtime.Methods
{
    /// <summary>
    /// Handles the "facet-call" framework execution method
    /// </summary>
    public static class FacetCall
    {
        /// <summary>
        /// Bootstrap a facet call
        /// </summary>
        public static JsonValue Start(
            JsonObject methodParameters,
            SpecialValues specialValues,
            Type[] gameAssemblyTypes
        )
        {
            // load or generate session id
            string sessionId = methodParameters["sessionId"].AsString
                ?? SandboxApiSession.GenerateSessionId();
            
            // send the session id back to the client
            specialValues.Add("sessionId", sessionId);

            // register session into container if not present already
            // TODO: nope, create some sort of configuration -> session driver
            if (!ServiceContainer.Default.CanResolve<ISession>())
            {
                ServiceContainer.Default.Register<ISession>(
                    new SandboxApiSession()
                );
            }
            
            FacetRequest request = FacetRequest.CreateFrom(
                methodParameters["facetName"],
                methodParameters["methodName"],
                methodParameters["arguments"],
                gameAssemblyTypes
            );

            MiddlewareAttribute[] globalMiddleware = {
                new MiddlewareAttribute(
                    typeof(SessionFacetMiddleware),
                    sessionId
                )
            };

            var response = FacetMiddleware.ExecuteMiddlewareStack(
                globalMiddleware,
                request,
                rq => {
                    object returnedValue = rq.Method.Invoke(
                        rq.Facet,
                        rq.Arguments
                    );

                    return FacetResponse.CreateFrom(
                        returnedValue,
                        request.Method
                    );
                }
            );

            return response.ReturnedJson;
        }
    }
}
