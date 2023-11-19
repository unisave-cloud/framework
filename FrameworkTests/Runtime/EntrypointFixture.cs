using System;
using LightJson;
using NUnit.Framework;
using Unisave.Runtime;

namespace FrameworkTests.Runtime
{
    public abstract class EntrypointFixture
    {
        private Type[] currentBackendTypes;
        
        /// <summary>
        /// You should call <see cref="SetEntrypointContext"/> method with the
        /// list of backend types to use here.
        /// </summary>
        [SetUp]
        public abstract void SetUpEntrypointContext();

        /// <summary>
        /// Call this to initialize the backend application.
        /// The application is initialized to run in an in-memory setup.
        /// </summary>
        /// <param name="backendTypes">
        /// Backend types to load, excluding the frameworks types.
        /// </param>
        protected virtual void SetEntrypointContext(
            Type[] backendTypes
        )
        {
            currentBackendTypes = backendTypes;
        }
        
        /// <summary>
        /// Execute the framework with execution parameters
        /// </summary>
        public EntrypointResponse Execute(
            string facetName,
            string methodName,
            JsonArray arguments,
            string sessionId = null,
            string env = ""
        )
        {
            if (currentBackendTypes == null)
                throw new NullReferenceException(
                    "Backend types need to be initialized"
                );

            // make the application run in-memory
            env = "ARANGO_DRIVER=memory\n" +
                  "SESSION_DRIVER=memory\n" +
                  "\n" + env;

            string executionParameters = new JsonObject()
                .Add("method", "facet-call")
                .Add("methodParameters", new JsonObject()
                    .Add("facetName", facetName)
                    .Add("methodName", methodName)
                    .Add("arguments", arguments)
                    .Add("sessionId", sessionId)
                )
                .Add("env", env)
                .ToString();
            
            // execute the framework
            string result = Entrypoint.Start(
                executionParameters,
                currentBackendTypes
            );

            return new EntrypointResponse(result);
        }
    }
}