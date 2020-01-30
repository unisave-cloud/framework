using System;
using LightJson;
using LightJson.Serialization;
using Unisave.Serialization;
using Unisave.Exceptions;
using Unisave.Database;
using Unisave.Runtime.Methods;
using Unisave.Services;

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
        public static string Start(
            string executionParametersAsJson,
            Type[] gameAssemblyTypes
        )
        {
            Console.WriteLine(
                "Starting Unisave framework " + FrameworkMeta.Version
            );

            JsonObject executionParameters = JsonReader.Parse(executionParametersAsJson);

            // extract arguments
            string executionMethod = executionParameters[nameof(executionMethod)];
            JsonValue methodParameters = executionParameters[nameof(methodParameters)];

            JsonValue methodResponse;

            try
            {
                // prepare the runtime environment
                BootUpServices();

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
                    .Add("exceptionAsString", e.InnerException?.ToString())
                    .Add("exception", Serializer.ToJson(e.InnerException))
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
                // unhandled exception coming from inside the bootstrapping logic
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

        // prevents teardown of testing-injected services
        private static bool servicesHaveBeenAlreadyPrepared;

        /// <summary>
        /// Initializes services, like database connection
        /// </summary>
        private static void BootUpServices()
        {
            // if we already have a container, it has probably been
            // given to us by some testing setup method <s>or it's perfectly
            // setup by some previous script executions.</s>
            //
            // so won't create any services, since they should already be there
            if (ServiceContainer.Default != null)
            {
                servicesHaveBeenAlreadyPrepared = true;
                return;
            }
            else
            {
                servicesHaveBeenAlreadyPrepared = false;
            }

            // create container and fill it with services
            var container = ServiceContainer.Default = new ServiceContainer();
            
            // database
            container.Register<IDatabase>(new Services.SandboxDatabaseApi());
        }

        /// <summary>
        /// Destroys all services and the container, before exiting
        /// </summary>
        private static void TearDownServices()
        {
            if (servicesHaveBeenAlreadyPrepared)
                return;
            
            ServiceContainer.Default?.Dispose();
            ServiceContainer.Default = null;
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
