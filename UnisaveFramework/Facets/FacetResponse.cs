using System.Reflection;
using LightJson;
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
            MethodInfo methodInfo
        )
        {
            var response = new FacetResponse();

            response.Returned = returned;
            response.ReturnedJson = ExecutionHelper
                .SerializeReturnValue(methodInfo, returned);

            return response;
        }
    }
}