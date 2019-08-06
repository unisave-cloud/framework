﻿using System;
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
        /// How did the bootstraping fail
        /// These codes are important outside of this assembly
        /// </summary>
        internal enum ErrorType
        {
            // 1xx ... nailing down the exact method to call
            FacetNameAmbiguous = 101,
            FacetNotFound = 102,
            MethodNameAmbiguous = 103,
            MethodDoesNotExist = 104,
            MethodNotPublic = 105,

            // 2xx ... something with arguments
            InvalidNumberOfArguments = 201,
            ArgumentDeserializationException = 202,

            // 3xx ... other
            FacetCreationException = 301,
            ReturnedValueSerializationException = 302,
        }

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
        ///     // how did the execution go? If there was an exception, was it caused by
        ///     // the game code, or worse by the boostrapping framework code?
        ///     "result": "ok" / "game-exception" / "error",
        /// 
        ///     // WHEN "ok"
        /// 
        ///     "hasReturnValue": true / false, // false if the method returned void
        ///     "returnValue": <?> // unisave json serialized
        /// 
        ///     // WHEN "game-exception"
        /// 
        ///     "exceptionAsString": "...",
        /// 
        ///     // WHEN "error"
        /// 
        ///     "errorType": see the ErrorType enumeration (not all errors need to be saved to logs)
        ///     "messageForUser": "This will be displayed to the user in unity editor",
        ///     "messageForLog": "This massive explanation will be put into unisave logs."
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
                ErrorType type;

                switch (e.Problem)
                {
                    case FacetSearchException.ProblemType.FacetNameAmbiguous:
                        type = ErrorType.FacetNameAmbiguous;
                        break;

                    case FacetSearchException.ProblemType.FacetNotFound:
                        type = ErrorType.FacetNotFound;
                        break;
                    
                    default:
                        // take down everything so that I notice and fix this
                        throw new NotImplementedException(
                            $"Method {nameof(Facet.FindFacetTypeByName)} threw an unknown error {e.Problem}.",
                            e
                        );
                }

                return Error(type, e.Message);
            }

            // find the requested method

            MethodInfo methodInfo;

            try
            {
                methodInfo = Facet.FindFacetMethodByName(facetType, methodName);
            }
            catch (FacetMethodSearchException e)
            {
                ErrorType type;

                switch (e.Problem)
                {
                    case FacetMethodSearchException.ProblemType.MethodNameAmbiguous:
                        type = ErrorType.MethodNameAmbiguous;
                        break;

                    case FacetMethodSearchException.ProblemType.MethodDoesNotExist:
                        type = ErrorType.MethodDoesNotExist;
                        break;

                    case FacetMethodSearchException.ProblemType.MethodNotPublic:
                        type = ErrorType.MethodNotPublic;
                        break;

                    default:
                        // take down everything so that I notice and fix this
                        throw new NotImplementedException(
                            $"Method {nameof(Facet.FindFacetMethodByName)} threw an unknown error {e.Problem}.",
                            e
                        );
                }

                return Error(type, e.Message);
            }
            
            // deserialize arguments

            ParameterInfo[] parameters = methodInfo.GetParameters(); // NOT including the "this" parameter

            if (parameters.Length != jsonArguments.Count)
                return Error(
                    ErrorType.InvalidNumberOfArguments,
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
                    return Error(
                        ErrorType.ArgumentDeserializationException,
                        $"Exception occured when deserializing the argument '{parameters[i].Name}':\n"
                        + e.ToString()
                    );
                }
            }

            // create facet instance

            Facet instance = null;

            try
            {
                instance = Facet.CreateInstance(facetType, new UnisavePlayer(callerId));
            }
            catch (Exception e)
            {
                return Error(
                    ErrorType.FacetCreationException,
                    $"Exception occured when creating the facet '{facetName}':\n"
                    + e.ToString()
                );
            }

            // call the facet method

            object returnedValue = null;

            try
            {
                returnedValue = methodInfo.Invoke(instance, deserializedArguments);
            }
            catch (TargetInvocationException e) // reflection puts a wrapper around any thrown exception
            {
                return GameException(e.InnerException);
            }

            // if returned void
            if (methodInfo.ReturnType == typeof(void))
            {
                var result = new JsonObject();
                result.Add("result", "ok");
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
                return Error(
                    ErrorType.ReturnedValueSerializationException,
                    $"Exception occured when serializing value returned from '{facetName}.{methodName}':\n"
                    + e.ToString()
                );
            }

            // build the response

            var response = new JsonObject();
            response.Add("result", "ok");
            response.Add("hasReturnValue", true);
            response.Add("returnValue", serializedReturnedValue);
            return response;
        }

        /// <summary>
        /// Formats an error response
        /// </summary>
        private static JsonObject Error(ErrorType type, string messageForUser, string messageForLog = null)
        {
            var result = new JsonObject();
            result.Add("result", "error");
            result.Add("errorType", (int)type);
            result.Add("messageForUser", messageForUser);
            result.Add("messageForLog", messageForLog ?? messageForUser);
            return result;
        }

        /// <summary>
        /// Formats an exception response
        /// </summary>
        private static JsonObject GameException(Exception e)
        {
            var result = new JsonObject();
            result.Add("result", "game-exception");
            result.Add("exceptionAsString", e.ToString());
            return result;
        }
    }
}