using System;
using System.Reflection;
using LightJson;
using Unisave.Exceptions;
using Unisave.Facets;
using Unisave.Utils;

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
            // TODO: generate or pass sessionId
            specialValues.Add("sessionId", "123456789");
            
            FacetRequest request = FacetRequest.CreateFrom(
                methodParameters["facetName"],
                methodParameters["methodName"],
                methodParameters["arguments"],
                gameAssemblyTypes
            );

            var response = FacetMiddleware.ExecuteMiddlewareStack(
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
