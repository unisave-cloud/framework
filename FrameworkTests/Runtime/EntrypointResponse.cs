using System;
using LightJson;
using LightJson.Serialization;
using NUnit.Framework;
using Unisave.Serialization;

namespace FrameworkTests.Runtime
{
    public class EntrypointResponse
    {
        public JsonObject Json { get; }

        public EntrypointResponse(string responseJson)
        {
            Json = JsonReader.Parse(responseJson).AsJsonObject;
            
            // assert ever present values
            AssertHasSpecial("executionDuration");
            AssertHasSpecial("sessionId");
            AssertHasSpecial("logs");
        }
        
        /// <summary>
        /// Asserts that no exception has been thrown
        /// </summary>
        public EntrypointResponse AssertSuccess()
        {
            Assert.AreEqual(
                "ok",
                Json["result"].AsString,
                "Execution has not succeeded."
            );
            return this;
        }
        
        /// <summary>
        /// Asserts that an exception has been thrown
        /// </summary>
        public EntrypointResponse AssertExceptionThrown()
        {
            Assert.AreEqual(
                "exception",
                Json["result"].AsString,
                "Execution has not thrown an exception."
            );
            return this;
        }

        /// <summary>
        /// Assert that the returned value from method matches given json string
        /// (also asserts success)
        /// </summary>
        public EntrypointResponse AssertReturned(JsonValue expectedReturned)
        {
            AssertSuccess();
            
            Assert.AreEqual(
                expectedReturned.ToString(true),
                Json["returned"].ToString(true)
            );
            
            return this;
        }

        /// <summary>
        /// Assert that there exists a special value
        /// </summary>
        public EntrypointResponse AssertHasSpecial(string key)
        {
            Assert.IsTrue(
                Json["special"].AsJsonObject.ContainsKey(key),
                $"Special value '{key}' is missing:\n"
                + Json["special"].ToString(true)
            );
            
            return this;
        }
        
        /// <summary>
        /// Assert that there exists a special value
        /// </summary>
        public EntrypointResponse AssertHasSpecial(
            string key,
            JsonValue expectedValue
        )
        {
            AssertHasSpecial(key);
            
            Assert.AreEqual(
                expectedValue.ToString(true),
                Json["special"][key].ToString(true)
            );
            
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
                Json["exception"]
            );
        }
    }
}