using System;
using System.Collections.Generic;
using System.Reflection;
using LightJson;
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
        public JsonArray JsonArguments { get; private set; }
        public object[] Arguments { get; private set; }
        public Facet Facet { get; private set; }
        
        private FacetRequest() { }

        public static FacetRequest CreateFrom(
            string facetName,
            string methodName,
            JsonArray jsonArguments,
            IEnumerable<Type> typesToSearch
        )
        {
            var request = new FacetRequest();

            request.FacetName = facetName;
            request.MethodName = methodName;
            request.JsonArguments = jsonArguments;
            
            request.FacetType = Facet.FindFacetTypeByName(
                facetName,
                typesToSearch
            );

            request.Method = ExecutionHelper.FindMethodByName(
                request.FacetType,
                methodName
            );

            request.Arguments = ExecutionHelper.DeserializeArguments(
                request.Method,
                jsonArguments
            );

            request.Facet = Facet.CreateInstance(request.FacetType);

            return request;
        }
    }
}