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
    public static class Entrypoint
    {
        /// <summary>
        /// Executes given server-side script based on the provided execution parameters
        /// Accepts execution parameters and returns execution result
        /// For more info see the internal documentation
        /// </summary>
        public static string Start(string executionParametersAsJson, Type[] gameAssemblyTypes)
        {
            Console.WriteLine(
                "Starting unisave framework " + typeof(Entrypoint).Assembly.GetName().Version.ToString(3)
            );

            JsonObject executionParameters = JsonReader.Parse(executionParametersAsJson);

            // extract arguments
            string executionId = executionParameters[nameof(executionId)];
            string databaseProxyIp = executionParameters[nameof(databaseProxyIp)];
            int databaseProxyPort = executionParameters[nameof(databaseProxyPort)];
            string executionMethod = executionParameters[nameof(executionMethod)];
            JsonValue methodParameters = executionParameters[nameof(methodParameters)];

            JsonValue methodResponse;

            try
            {
                // prepare the runtime environment
                BootUpServices(executionId, databaseProxyIp, databaseProxyPort);

                // do the business
                methodResponse = ExecuteProperMethod(executionMethod, methodParameters, gameAssemblyTypes);

                // tear down the runtime environment
                TearDownServices();
            }
            catch (GameScriptException e)
            {
                // game code threw en exception
                // propagate the exception to the client
                return new JsonObject()
                    .Add("result", "exception")
                    .Add("exceptionAsString", e.InnerException.ToString())
                    .ToString();
            }
            catch (InvalidMethodParametersException e)
            {
                // method invocation failed even before the target code was executed
                return new JsonObject()
                    .Add("result", "invalid-method-parameters")
                    .Add("message", e.Message + (e.InnerException == null ? "" : "\n" + e.InnerException.ToString()))
                    .ToString();
            }
            catch (Exception e)
            {
                // unhandled exception comming from inside the bootstrapping logic
                // this is bad
                return new JsonObject()
                    .Add("result", "error")
                    .Add("errorMessage", "Unhandled exception caught inside Entrypoint.Start:\n" + e.ToString())
                    .ToString();
            }

            // successful execution
            return new JsonObject()
                .Add("result", "ok")
                .Add("methodResponse", methodResponse)
                .ToString();
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
            // database
            if (databaseProxyIp != null)
            {
                var database = new UnisaveDatabase();
                database.Connect(executionId, databaseProxyIp, databaseProxyPort);
                Endpoints.DatabaseResolver = () => database;
            }
        }

        /// <summary>
        /// Destroys all services, before exiting
        /// </summary>
        private static void TearDownServices()
        {
            // database
            if (Endpoints.DatabaseResolver != null)
            {
                ((UnisaveDatabase)Endpoints.Database).Disconnect();
                Endpoints.DatabaseResolver = null;
            }
        }

        /// <summary>
        /// Select proper execution method and handle it
        /// </summary>
        private static JsonValue ExecuteProperMethod(
            string executionMethod, JsonValue methodParameters, Type[] gameAssemblyTypes
        )
        {
            switch (executionMethod)
            {
                case "facet":
                    return FacetCall.Start(methodParameters, gameAssemblyTypes);

                case "migration":
                    return MigrationCall.Start(methodParameters, gameAssemblyTypes);

                case "player-registration-hook":
                    return PlayerRegistrationHookCall.Start(methodParameters, gameAssemblyTypes);

                default:
                    throw new UnisaveException($"UnisaveFramework: Unknown execution method: {executionMethod}");
            }
        }
    }
}
