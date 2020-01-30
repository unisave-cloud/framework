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
    /// Here is where all execution begins
    /// </summary>
    public static class Entrypoint
    {
        /// <summary>
        /// Should uncaught exceptions, raised during the execution,
        /// be serialized and returned as a response, or should they
        /// be left to propagate upwards?
        /// </summary>
        internal static bool SerializeExceptions { get; set; } = true;

        /// <summary>
        /// Used to detect recursion in framework executions
        /// </summary>
        private static bool someExecutionIsRunning;
        
        /// <summary>
        /// Should the service container be disposed after execution finishes
        /// </summary>
        private static bool tearDownServices;

        /// <summary>
        /// Parsed execution method name
        /// </summary>
        private static string methodName;
        
        /// <summary>
        /// Parsed execution method parameters
        /// </summary>
        private static JsonValue methodParameters;

        /// <summary>
        /// Special values object built up during execution
        /// </summary>
        private static SpecialValues specialValues;

        /// <summary>
        /// Main entrypoint to the framework execution
        /// Given executionParameters it starts some action which results.
        /// in a returned value or an exception.
        /// 
        /// Input and output are serialized as JSON strings.
        /// </summary>
        public static string Start(
            string executionParametersAsJson,
            Type[] gameAssemblyTypes
        )
        {
            if (someExecutionIsRunning)
                throw new InvalidOperationException(
                    "You cannot start the framework from within the framework."
                );
            
            someExecutionIsRunning = true;

            try
            {
                PrintGreeting();

                ParseExecutionParameters(executionParametersAsJson);

                specialValues = new SpecialValues();

                JsonObject result = HandleExceptionSerialization(() => {

                    BootUpServices();

                    JsonValue methodResult = ExecuteProperMethod(
                        gameAssemblyTypes
                    );

                    TearDownServices();

                    return new JsonObject()
                        .Add("result", "ok")
                        .Add("returned", methodResult)
                        .Add("special", specialValues.ToJsonObject());
                });

                return result.ToString();
            }
            finally
            {
                someExecutionIsRunning = false;
            }
        }

        /// <summary>
        /// Prints framework greeting to the commandline
        /// </summary>
        private static void PrintGreeting()
        {
            Console.WriteLine(
                "Starting Unisave framework " + FrameworkMeta.Version
            );
        }

        /// <summary>
        /// Parses execution parameters JSON string
        /// </summary>
        private static void ParseExecutionParameters(string executionParameters)
        {
            JsonObject ep = JsonReader.Parse(executionParameters);

            methodName = ep["method"];
            methodParameters = ep["methodParameters"];
        }

        /// <summary>
        /// Initializes services (e.g. database connection)
        /// </summary>
        private static void BootUpServices()
        {
            // If we already have a container, it has probably been
            // given to us by some testing setup method so don't touch it
            if (ServiceContainer.Default != null)
            {
                tearDownServices = false;
                return;
            }
            
            tearDownServices = true;
            
            // create container and fill it with services
            var container = ServiceContainer.Default = new ServiceContainer();
            
            // database
            container.Register<IDatabase>(new SandboxDatabaseApi());
        }

        /// <summary>
        /// Destroys all services and the container
        /// </summary>
        private static void TearDownServices()
        {
            if (!tearDownServices)
                return;
            
            ServiceContainer.Default?.Dispose();
            ServiceContainer.Default = null;
        }

        /// <summary>
        /// Handles serialization of exceptions that
        /// might be thrown by the provided lambda
        /// </summary>
        private static JsonObject HandleExceptionSerialization(
            Func<JsonObject> action
        )
        {
            try
            {
                return action.Invoke();
            }
            catch (GameScriptException e)
            {
                // TODO: remove these funky GameScriptExceptions
                // (to preserve stacktrace, see OnFacet testing helper)
                
                if (!SerializeExceptions)
                    throw;
                
                return new JsonObject()
                    .Add("result", "exception")
                    .Add("exception", Serializer.ToJson(e.InnerException))
                    .Add("special", specialValues.ToJsonObject());
            }
            catch (Exception e)
            {
                if (!SerializeExceptions)
                    throw;
                
                return new JsonObject()
                    .Add("result", "exception")
                    .Add("exception", Serializer.ToJson(e))
                    .Add("special", specialValues.ToJsonObject());
            }
        }
        
        /// <summary>
        /// Select proper execution method and handle it
        /// </summary>
        private static JsonValue ExecuteProperMethod(Type[] gameAssemblyTypes)
        {
            switch (methodName)
            {
                case "entrypoint-test":
                    return EntrypointTest.Start(
                        methodParameters,
                        specialValues
                    );
                
                case "facet-call":
                    return FacetCall.Start(
                        methodParameters,
                        specialValues,
                        gameAssemblyTypes
                    );

                case "migration":
                    return MigrationCall.Start(
                        methodParameters,
                        gameAssemblyTypes
                    );

                case "player-registration-hook":
                    return PlayerRegistrationHookCall.Start(
                        methodParameters,
                        gameAssemblyTypes
                    );

                default:
                    throw new UnisaveException(
                        "UnisaveFramework: Unknown execution method: "
                        + methodName
                    );
            }
        }
    }
}
