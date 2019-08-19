using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LightJson;
using LightJson.Serialization;
using Unisave.Serialization;
using Unisave.Exceptions;
using Unisave.Database;

namespace Unisave.Runtime
{
    /// <summary>
    /// Handles the "facet" execution method of server-side code execution
    /// </summary>
    public static class FacetCall
    {
        /// <summary>
        /// Bootstrap a facet call
        /// </summary>
        /// <param name="executionParameters">
        /// Execution parameters as json
        /// {
        ///     "facetName": "MyFacet",
        ///     "methodName": "DoCoolStuff",
        ///     "arguments": [...], // serialized by the unisave serialization system
        ///     "callerId": "...", // id of the calling player
        /// }    
        /// </param>
        /// <param name="gameAssemblyTypes">
        /// Game assembly types to look through to find the requested facet class
        /// </param>
        /// <returns>
        /// Json string containing result of the execution.
        /// {
        ///     "hasReturnValue": true / false, // false if the method returned void
        ///     "returnValue": <?> // unisave json serialized
        /// }
        /// </returns>
        public static JsonObject Start(JsonObject executionParameters, Type[] gameAssemblyTypes)
        {
            // extract arguments

            string facetName = executionParameters["facetName"];
            string methodName = executionParameters["methodName"];
            JsonArray jsonArguments = executionParameters["arguments"];
            string callerId = executionParameters["callerId"];

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
            MethodInfo methodInfo;
            JsonValue returnedValue;
            try
            {
                returnedValue = ExecutionHelper.ExecuteMethod(
                    facet,
                    methodName,
                    jsonArguments,
                    out methodInfo
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

            // build the response
            return new JsonObject()
                .Add("hasReturnValue", methodInfo.ReturnType != typeof(void))
                .Add("returnValue", returnedValue);
        }
    }
}
