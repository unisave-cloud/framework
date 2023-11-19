using System;
using System.Collections.Generic;
using System.Reflection;
using LightJson;
using Unisave.Foundation;
using Unisave.Utils;

namespace Unisave.Facets
{
    /// <summary>
    /// Represents the request for a facet to be called
    /// </summary>
    public class FacetRequest
    {
        public string FacetName { get; private set; }
        public Type FacetType { get; private set; }
        public string MethodName { get; private set; }
        public MethodInfo Method { get; private set; }
        public JsonArray Arguments { get; private set; }
        
        private FacetRequest() { }

        public static FacetRequest CreateFrom(
            string facetName,
            string methodName,
            JsonArray jsonArguments,
            BackendTypes backendTypes
        )
        {
            var request = new FacetRequest {
                FacetName = facetName,
                MethodName = methodName,
                Arguments = jsonArguments,
                FacetType = Facet.FindFacetTypeByName(
                    facetName,
                    backendTypes
                )
            };

            request.Method = Facet.FindMethodByName(
                request.FacetType,
                methodName
            );

            return request;
        }
    }
}