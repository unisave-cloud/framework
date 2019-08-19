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

            // find the requested method

            MethodInfo methodInfo;

            try
            {
                methodInfo = Facet.FindFacetMethodByName(facetType, methodName);
            }
            catch (FacetMethodSearchException e)
            {
                throw new InvalidMethodParametersException("Facet method wasn't found.", e);
            }
            
            // deserialize arguments

            ParameterInfo[] parameters = methodInfo.GetParameters(); // NOT including the "this" parameter

            if (parameters.Length != jsonArguments.Count)
                throw new InvalidMethodParametersException(
                    $"Method '{facetName}.{methodName}' accepts different number of arguments than provided. "
                    + "Make sure you don't use the params keyword or default argument values, "
                    + "since it is not supported by Unisave."
                );

            object[] deserializedArguments = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                try
                {
                    deserializedArguments[i] = Loader.Load(jsonArguments[i], parameters[i].ParameterType);
                }
                catch (Exception e)
                {
                    throw new InvalidMethodParametersException(
                        $"Exception occured when deserializing the argument '{parameters[i].Name}'", e
                    );
                }
            }

            // create facet instance

            Facet instance = Facet.CreateInstance(facetType, new UnisavePlayer(callerId));

            // call the facet method

            object returnedValue = null;

            try
            {
                returnedValue = methodInfo.Invoke(instance, deserializedArguments);
            }
            catch (TargetInvocationException e) // reflection puts a wrapper around any thrown exception
            {
                throw new GameScriptException(e.InnerException);
            }

            // if returned void
            if (methodInfo.ReturnType == typeof(void))
            {
                var result = new JsonObject();
                result.Add("hasReturnValue", false);
                return result;
            }

            // serialize returned value

            JsonValue serializedReturnedValue = JsonValue.Null;

            try
            {
                serializedReturnedValue = Saver.Save(returnedValue);
            }
            catch (Exception e)
            {
                throw new InvalidMethodParametersException(
                    new UnisaveException(
                        $"Exception occured when serializing value returned from '{facetName}.{methodName}'",
                        e
                    )
                );
            }

            // build the response

            return new JsonObject()
                .Add("hasReturnValue", true)
                .Add("returnValue", serializedReturnedValue);
        }
    }
}
