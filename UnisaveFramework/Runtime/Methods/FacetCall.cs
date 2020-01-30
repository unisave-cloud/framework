using System;
using System.Reflection;
using LightJson;
using Unisave.Exceptions;
using Unisave.Utils;

namespace Unisave.Runtime.Methods
{
    /// <summary>
    /// Handles the "facet" execution method of server-side code execution
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
            
            // extract arguments
            string facetName = methodParameters["facetName"];
            string methodName = methodParameters["methodName"];
            JsonArray jsonArguments = methodParameters["arguments"];
            string callerId = methodParameters["callerId"];

            // find the requested facet
            Type facetType;
            try
            {
                facetType = Facet.FindFacetTypeByName(facetName, gameAssemblyTypes);
            }
            catch (FacetSearchException e)
            {
                throw new InvalidMethodParametersException("Facet class wasn't found.", e);
            }

            // create facet instance
            Facet facet = Facet.CreateInstance(facetType, new UnisavePlayer(callerId));

            // execute the method
            try
            {
                return ExecutionHelper.ExecuteMethod(
                    facet,
                    methodName,
                    jsonArguments,
                    out MethodInfo methodInfo
                );
            }
            catch (MethodSearchException e)
            {
                throw new InvalidMethodParametersException(e);
            }
            catch (ExecutionSerializationException e)
            {
                throw new InvalidMethodParametersException(e);
            }
            catch (TargetInvocationException e)
            {
                throw new GameScriptException(e.InnerException);
            }
        }
    }
}
