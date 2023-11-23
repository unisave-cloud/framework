using System;
using System.Reflection;
using LightJson;
using Unisave.Serialization;
using Unisave.Serialization.Context;
using Unisave.Utils;

namespace Unisave.Facets
{
    /// <summary>
    /// Represents the response from a facet call
    /// 
    /// IMPORTANT: This does not cover exceptions,
    /// those are left to propagate and serialized
    /// at the top-most level of the framework
    /// </summary>
    public class FacetResponse
    {
        public object Returned { get; private set; }
        public JsonValue ReturnedJson { get; private set; }

        private FacetResponse() { }
        
        public static FacetResponse CreateFrom(
            object returned,
            Type returnType
        )
        {
            return new FacetResponse {
                Returned = returned,
                ReturnedJson = Facet.SerializeReturnedValue(
                    returnType,
                    returned
                )
            };
        }
    }
}