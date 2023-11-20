using System;
using LightJson;
using NUnit.Framework;
using Unisave.Facades;
using Unisave.Facets;

namespace FrameworkTests.Runtime
{
    [TestFixture]
    public class EntrypointLoggingTest : EntrypointFixture
    {
        #region "Backend definition"

        public class MyFacet : Facet
        {
            public void LogInfo(string message)
            {
                Log.Info(message);
            }
            
            public void LogWithException(string message)
            {
                Log.Info(message);
                
                throw new Exception("Duh");
            }
        }
        
        #endregion
        
        public override void SetUpEntrypointContext()
        {
            SetEntrypointContext(new Type[] {
                typeof(MyFacet)
            });
        }
        
        [Test]
        public void ItCanLogInfo()
        {
            EntrypointResponse response = Execute(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.LogInfo),
                arguments: new JsonArray() { "Hello world!" }
            );
            response.AssertReturned(JsonValue.Null);
            JsonArray logs = response.Json["special"]["logs"];
            
            Assert.AreEqual(1, logs.Count);
            JsonObject record = logs[0];
            
            Assert.AreEqual("info", record["level"].AsString);
            Assert.IsTrue(record.ContainsKey("time"));
            Assert.AreEqual("Hello world!", record["message"].AsString);
            Assert.IsTrue(record["context"].IsNull);
        }
        
        [Test]
        public void ItCanLogWithException()
        {
            EntrypointResponse response = Execute(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.LogWithException),
                arguments: new JsonArray() { "Hello world!" }
            );
            response.AssertExceptionThrown();
            JsonArray logs = response.Json["special"]["logs"];
            
            Assert.AreEqual(1, logs.Count);
            JsonObject record = logs[0];
            
            Assert.AreEqual("info", record["level"].AsString);
            Assert.IsTrue(record.ContainsKey("time"));
            Assert.AreEqual("Hello world!", record["message"].AsString);
            Assert.IsTrue(record["context"].IsNull);
        }
    }
}