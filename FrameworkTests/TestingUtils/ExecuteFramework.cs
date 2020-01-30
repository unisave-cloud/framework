using System;
using System.Collections.Generic;
using System.Linq;
using LightJson;
using LightJson.Serialization;
using NUnit.Framework;
using Unisave;
using Unisave.Runtime;
using Unisave.Serialization;
using Unisave.Services;

namespace FrameworkTests.TestingUtils
{
    /// <summary>
    /// Helper to execute framework from the top-most level
    /// (by using execution parameters serialized via JSON)
    /// </summary>
    public class ExecuteFramework
    {
        private IEnumerable<Type> types;
        private bool serializeExceptions;
        private ServiceContainer serviceContainer;
        
        /// <summary>
        /// Creates the framework execution description
        /// and allows you to chain more commands
        /// </summary>
        public static ExecuteFramework Begin()
        {
            return new ExecuteFramework();
        }

        /// <summary>
        /// Set game assembly types parameter
        /// </summary>
        public ExecuteFramework WithTypes(IEnumerable<Type> types)
        {
            this.types = types;
            return this;
        }

        /// <summary>
        /// Actually serialize exceptions as it should be done
        /// instead of letting them pass through
        /// </summary>
        public ExecuteFramework SerializeExceptions()
        {
            serializeExceptions = true;
            return this;
        }

        /// <summary>
        /// Set the service container to use
        /// </summary>
        public ExecuteFramework WithServiceContainer(ServiceContainer container)
        {
            serviceContainer = container;
            return this;
        }

        /// <summary>
        /// Execute the framework with execution parameters
        /// </summary>
        public FrameworkExecutionResult Execute(string executionParameters)
        {
            // When no types specified, use the entire testing project
            // and the framework project to nicely pollute the scope.
            // (pollute more than it would be in production)
            if (types == null)
            {
                types = typeof(ExecuteFramework).Assembly.GetTypes()
                    .Concat(typeof(FrameworkMeta).Assembly.GetTypes());
            }
            
            // NOTE: Sandbox api is not tested here,
            // instead it's tested on per service basis.
            // So there's no need to set it up.

            // set service container
            ServiceContainer.Default = serviceContainer;

            // set exception serialization
            Entrypoint.SerializeExceptions = serializeExceptions;
            
            // execute the framework
            string result = Entrypoint.Start(
                executionParameters,
                types.ToArray()
            );

            return new FrameworkExecutionResult(result);
        }
    }

    /// <summary>
    /// Contains result of a framework execution
    /// and lets you make assertion on it
    /// </summary>
    public class FrameworkExecutionResult
    {
        private string actualResult;
        private JsonValue actualJsonResult;
        
        public FrameworkExecutionResult(string actualResult)
        {
            this.actualResult = actualResult;
            actualJsonResult = JsonReader.Parse(actualResult);
        }

        /// <summary>
        /// Asserts that no exception has been thrown
        /// </summary>
        public FrameworkExecutionResult AssertSuccess()
        {
            Assert.AreEqual(
                "ok",
                actualJsonResult["result"].AsString,
                "Execution has not succeeded."
            );
            return this;
        }
        
        /// <summary>
        /// Asserts that an exception has been thrown
        /// </summary>
        public FrameworkExecutionResult AssertExceptionThrown()
        {
            Assert.AreEqual(
                "exception",
                actualJsonResult["result"].AsString,
                "Execution has not thrown an exception."
            );
            return this;
        }

        /// <summary>
        /// Assert that the returned value from method matches given json string
        /// (also asserts success)
        /// </summary>
        public FrameworkExecutionResult AssertReturned(string expectedReturned)
        {
            AssertSuccess();
            
            JsonValue expectedJsonReturned = JsonReader.Parse(expectedReturned);
            
            Assert.AreEqual(
                expectedJsonReturned.ToString(true),
                actualJsonResult["returned"].ToString(true)
            );
            
            return this;
        }
        
        /// <summary>
        /// Assert that the execution result matches
        /// provided json string exactly
        /// </summary>
        public FrameworkExecutionResult AssertResultIs(string expectedResult)
        {
            JsonValue expectedJsonResult = JsonReader.Parse(expectedResult);
            
            Assert.AreEqual(
                expectedJsonResult.ToString(true),
                actualJsonResult.ToString(true)
            );
            
            return this;
        }

        /// <summary>
        /// Assert that there exists a special value
        /// </summary>
        public FrameworkExecutionResult AssertHasSpecial(string key)
        {
            Assert.IsTrue(
                actualJsonResult["special"].AsJsonObject.ContainsKey(key),
                $"Special value '{key}' is missing:\n"
                + actualJsonResult["special"].ToString(true)
            );
            
            return this;
        }
        
        /// <summary>
        /// Assert that there exists a special value
        /// </summary>
        public FrameworkExecutionResult AssertHasSpecial(
            string key,
            string expectedValue
        )
        {
            AssertHasSpecial(key);
            
            JsonValue expectedJsonValue = JsonReader.Parse(expectedValue);
            
            Assert.AreEqual(
                expectedJsonValue.ToString(true),
                actualJsonResult["special"][key].ToString(true)
            );
            
            return this;
        }

        /// <summary>
        /// Asserts that the thrown exception is of given type
        /// </summary>
        public FrameworkExecutionResult AssertExceptionIs<T>()
        {
            var e = GetException();
            
            Assert.IsInstanceOf<T>(e);
            
            return this;
        }

        /// <summary>
        /// Returns the thrown exception
        /// Fails assertion if no exception thrown
        /// </summary>
        public Exception GetException()
        {
            AssertExceptionThrown();

            return Serializer.FromJson<Exception>(
                actualJsonResult["exception"]
            );
        }
    }
}