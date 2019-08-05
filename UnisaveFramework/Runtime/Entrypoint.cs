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
        /// For more info see the internal documentation
        /// </summary>
        public static string Start(string executionParametersAsJson, Type[] gameAssemblyTypes)
        {
            JsonObject executionParameters = JsonReader.Parse(executionParametersAsJson);

            // extract arguments

            string executionId = executionParameters[nameof(executionId)];
            string databaseProxyIp = executionParameters[nameof(databaseProxyIp)];
            int databaseProxyPort = executionParameters[nameof(databaseProxyPort)];
            string executionMethod = executionParameters[nameof(executionMethod)];
            JsonValue methodParameters = executionParameters[nameof(methodParameters)];

            // prepare the runtime environment

            BootUpServices(executionId, databaseProxyIp, databaseProxyPort);

            // branch by execution method

            JsonObject response;

            switch (executionMethod)
            {
                case "facet":
                    response = FacetCall.Start(methodParameters, gameAssemblyTypes);
                    break;

                case "migration":
                    response = MigrationCall.Start(methodParameters, gameAssemblyTypes);
                    break;

                default:
                    response = JsonValue.Null;
                    Console.WriteLine($"UnisaveFramework: Unknown execution method: {executionMethod}");
                    break;
            }

            // tear down the runtime environment

            TearDownServices();

            // return response

            return response.ToString();
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
    }
}
