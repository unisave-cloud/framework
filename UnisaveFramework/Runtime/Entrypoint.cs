using System;
using LightJson;
using Unisave.Serialization;
using Unisave.Exceptions;
using Unisave.Foundation;
using Unisave.Runtime.Kernels;
using Unisave.Runtime.Methods;

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

                var executionParameters = ExecutionParameters
                    .Parse(executionParametersAsJson);

                var specialValues = new SpecialValues();

                JsonObject result = HandleExceptionSerialization(
                    specialValues,
                    () => {
                        
                        // NOTE: this place is where a test/emulation
                        // is started. Simply boot the application and
                        // then directly call proper kernel (method handler).

                        var env = Env.Parse(executionParameters.EnvSource);
                        
                        using (var app = Bootstrap.Boot(
                            gameAssemblyTypes,
                            env,
                            specialValues
                        ))
                        {
                            Facade.SetApplication(app);
                            
                            JsonValue methodResult = ExecuteProperMethod(
                                executionParameters,
                                app
                            );
                            
                            Facade.SetApplication(null);
                            
                            return new JsonObject()
                                .Add("result", "ok")
                                .Add("returned", methodResult)
                                .Add("special", specialValues.ToJsonObject());
                        }
                        
                    }
                );

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
        /// Handles serialization of exceptions that
        /// might be thrown by the provided lambda
        /// </summary>
        private static JsonObject HandleExceptionSerialization(
            SpecialValues specialValues,
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
        private static JsonValue ExecuteProperMethod(
            ExecutionParameters executionParameters,
            Application app
        )
        {
            switch (executionParameters.Method)
            {
                case "entrypoint-test":
                    return EntrypointTest.Start(
                        executionParameters.MethodParameters,
                        app.Resolve<SpecialValues>()
                    );
                
                case "facet-call":
                    var kernel = app.Resolve<FacetCallKernel>();
                    var parameters = FacetCallKernel.MethodParameters
                        .Parse(executionParameters.MethodParameters);
                    return kernel.Handle(parameters);

                case "migration":
                    return MigrationCall.Start(
                        executionParameters.MethodParameters,
                        app.GameAssemblyTypes
                    );

                case "player-registration-hook":
                    return PlayerRegistrationHookCall.Start(
                        executionParameters.MethodParameters,
                        app.GameAssemblyTypes
                    );

                default:
                    throw new UnisaveException(
                        "UnisaveFramework: Unknown execution method: "
                        + executionParameters.Method
                    );
            }
        }
    }
}
