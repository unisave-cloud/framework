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
    /// Handles server-side code bootstrapping
    /// </summary>
    internal static class Bootstrap
    {
        /// <summary>
        /// Used for testing. We don't want to connect to a database when testing
        /// </summary>
        internal static bool ignoreServiceBooting = false;

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

        /*
            This code gets executed by some process that handles server-side code execution.
            We are already sandboxed at this point.
         */

        /// <summary>
        /// Bootstrap a facet call
        /// </summary>
        /// <param name="executionParametersAsJson">
        /// Execution parameters as json string
        /// {
        ///     "facetName": "MyFacet",
        ///     "methodName": "DoCoolStuff",
        ///     "arguments": [...], // serialized by the unisave serialization system
        ///     "callerId": "...", // id of the calling player
        ///     "executionId": "...", // id of this facet call execution
        ///     "databaseProxyIp": "...",
        ///     "databaseProxyPort": 0000,
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
        ///     "errorType": see the Bootstrap.ErrorType enumeration (not all error need to be saved to logs)
        ///     "messageForUser": "This will be displayed to the user in unity editor",
        ///     "messageForLog": "This massive explanation will be put into unisave logs."
        /// }    
        /// </returns>
        public static string FacetCall(string executionParametersAsJson, Type[] gameAssemblyTypes)
        {
            JsonObject executionParameters = JsonReader.Parse(executionParametersAsJson);

            // extract arguments

            string facetName = executionParameters["facetName"];
            string methodName = executionParameters["methodName"];
            JsonArray jsonArguments = executionParameters["arguments"];
            string callerId = executionParameters["callerId"];
            string executionId = executionParameters["executionId"];
            string databaseProxyIp = executionParameters["databaseProxyIp"];
            int databaseProxyPort = executionParameters["databaseProxyPort"];

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

            // boot up the rest of the system

            BootUpServices(
                executionId,
                databaseProxyIp,
                databaseProxyPort
            );

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

            // destroy the rest of the system

            TearDownServices();

            // if returned void
            if (methodInfo.ReturnType == typeof(void))
            {
                var result = new JsonObject();
                result.Add("result", "ok");
                result.Add("hasReturnValue", false);
                return result.ToString();
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
            return response.ToString();
        }

        /// <summary>
        /// Formats an error response
        /// </summary>
        private static string Error(ErrorType type, string messageForUser, string messageForLog = null)
        {
            var result = new JsonObject();
            result.Add("result", "error");
            result.Add("errorType", (int)type);
            result.Add("messageForUser", messageForUser);
            result.Add("messageForLog", messageForLog ?? messageForUser);
            return result.ToString();
        }

        /// <summary>
        /// Formats an exception response
        /// </summary>
        private static string GameException(Exception e)
        {
            var result = new JsonObject();
            result.Add("result", "game-exception");
            result.Add("exceptionAsString", e.ToString());
            return result.ToString();
        }

        /// <summary>
        /// Initializes services, like database connection
        /// </summary>
        private static void BootUpServices(
            string executionId,
            string databaseProxyIp,
            int databaseProxyPort
        )
        {
            if (ignoreServiceBooting)
                return;

            // database
            UnisaveDatabase.Instance = new UnisaveDatabase();
            UnisaveDatabase.Instance.Connect(executionId, databaseProxyIp, databaseProxyPort);
        }

        /// <summary>
        /// Destroys all services, before exiting
        /// </summary>
        private static void TearDownServices()
        {
            if (ignoreServiceBooting)
                return;

            // database
            if (UnisaveDatabase.Instance != null)
            {
                UnisaveDatabase.Instance.Disconnect();
                UnisaveDatabase.Instance = null;
            }
        }
    }
}
