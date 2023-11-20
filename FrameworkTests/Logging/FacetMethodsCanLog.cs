using System;
using System.Threading.Tasks;
using FrameworkTests.Testing;
using FrameworkTests.Testing.Facets;
using FrameworkTests.Testing.Foundation;
using LightJson;
using Microsoft.Owin;
using NUnit.Framework;
using Unisave.Facades;
using Unisave.Facets;
using UnityEngine;

namespace FrameworkTests.Logging
{
    [TestFixture]
    public class FacetMethodsCanLog : BackendApplicationFixture
    {
        #region "Backend definition"

        public class MyFacet : Facet
        {
            public void LogInfo(string message)
            {
                Log.Info(message);
            }
            
            public void LogWarning(string message)
            {
                Log.Warning(message);
            }
            
            public void LogError(string message)
            {
                Log.Error(message);
            }
            
            public void LogCritical(string message)
            {
                Log.Critical(message);
            }

            public void LogContext(string context)
            {
                Log.Info("Message!", context);
            }

            public void DebugLog(string message)
            {
                Debug.Log(message);
            }

            public void LogMany(string[] messages)
            {
                foreach (string message in messages)
                    Log.Info(message);
            }
            
            public void LogWithException(string message)
            {
                Log.Info(message);
                
                throw new Exception("Duh");
            }
        }
        
        #endregion
        
        public override void SetUpBackendApplication()
        {
            CreateApplication(new Type[] {
                typeof(MyFacet)
            });
        }

        [Test]
        public async Task ItCanLogInfo()
        {
            IOwinResponse response = await app.CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.LogInfo),
                arguments: new JsonArray() { "Hello world!" }
            );
            await response.AssertOkVoidResponse();
            JsonObject body = await response.ReadJsonBody<JsonObject>();
            JsonArray logs = body["logs"];
            
            Assert.AreEqual(1, logs.Count);
            JsonObject record = logs[0];
            
            Assert.AreEqual("info", record["level"].AsString);
            Assert.IsTrue(record.ContainsKey("time"));
            Assert.AreEqual("Hello world!", record["message"].AsString);
            Assert.IsTrue(record["context"].IsNull);
        }
        
        [Test]
        public async Task ItCanLogWarning()
        {
            IOwinResponse response = await app.CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.LogWarning),
                arguments: new JsonArray() { "Hello world!" }
            );
            await response.AssertOkVoidResponse();
            JsonObject body = await response.ReadJsonBody<JsonObject>();
            JsonArray logs = body["logs"];
            
            Assert.AreEqual(1, logs.Count);
            JsonObject record = logs[0];
            
            Assert.AreEqual("warning", record["level"].AsString);
            Assert.IsTrue(record.ContainsKey("time"));
            Assert.AreEqual("Hello world!", record["message"].AsString);
            Assert.IsTrue(record["context"].IsNull);
        }
        
        [Test]
        public async Task ItCanLogError()
        {
            IOwinResponse response = await app.CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.LogError),
                arguments: new JsonArray() { "Hello world!" }
            );
            await response.AssertOkVoidResponse();
            JsonObject body = await response.ReadJsonBody<JsonObject>();
            JsonArray logs = body["logs"];
            
            Assert.AreEqual(1, logs.Count);
            JsonObject record = logs[0];
            
            Assert.AreEqual("error", record["level"].AsString);
            Assert.IsTrue(record.ContainsKey("time"));
            Assert.AreEqual("Hello world!", record["message"].AsString);
            Assert.IsTrue(record["context"].IsNull);
        }
        
        [Test]
        public async Task ItCanLogCritical()
        {
            IOwinResponse response = await app.CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.LogCritical),
                arguments: new JsonArray() { "Hello world!" }
            );
            await response.AssertOkVoidResponse();
            JsonObject body = await response.ReadJsonBody<JsonObject>();
            JsonArray logs = body["logs"];
            
            Assert.AreEqual(1, logs.Count);
            JsonObject record = logs[0];
            
            Assert.AreEqual("critical", record["level"].AsString);
            Assert.IsTrue(record.ContainsKey("time"));
            Assert.AreEqual("Hello world!", record["message"].AsString);
            Assert.IsTrue(record["context"].IsNull);
        }
        
        [Test]
        public async Task ItCanLogContext()
        {
            IOwinResponse response = await app.CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.LogContext),
                arguments: new JsonArray() { "some context" }
            );
            await response.AssertOkVoidResponse();
            JsonObject body = await response.ReadJsonBody<JsonObject>();
            JsonArray logs = body["logs"];
            
            Assert.AreEqual(1, logs.Count);
            JsonObject record = logs[0];
            
            Assert.AreEqual("info", record["level"].AsString);
            Assert.IsTrue(record.ContainsKey("time"));
            Assert.AreEqual("Message!", record["message"].AsString);
            Assert.AreEqual("some context", record["context"].AsString);
        }
        
        [Test]
        public async Task ItCanUseUnityDebugLog()
        {
            IOwinResponse response = await app.CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.DebugLog),
                arguments: new JsonArray() { "Hello world!" }
            );
            await response.AssertOkVoidResponse();
            JsonObject body = await response.ReadJsonBody<JsonObject>();
            JsonArray logs = body["logs"];
            
            Assert.AreEqual(1, logs.Count);
            JsonObject record = logs[0];
            
            Assert.AreEqual("info", record["level"].AsString);
            Assert.IsTrue(record.ContainsKey("time"));
            Assert.AreEqual("Hello world!", record["message"].AsString);
            Assert.IsTrue(record["context"].IsNull);
        }
        
        [Test]
        public async Task ItCanLogMultipleMessages()
        {
            IOwinResponse response = await app.CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.LogMany),
                arguments: new JsonArray() {
            new JsonArray() { "First!", "Second!", "Third!" }
                }
            );
            await response.AssertOkVoidResponse();
            JsonObject body = await response.ReadJsonBody<JsonObject>();
            JsonArray logs = body["logs"];
            
            Assert.AreEqual(3, logs.Count);
            
            Assert.AreEqual("First!", logs[0]["message"].AsString);
            Assert.AreEqual("Second!", logs[1]["message"].AsString);
            Assert.AreEqual("Third!", logs[2]["message"].AsString);
        }
        
        [Test]
        public async Task ItCanLogWithException()
        {
            IOwinResponse response = await app.CallFacet(
                facetName: typeof(MyFacet).FullName,
                methodName: nameof(MyFacet.LogWithException),
                arguments: new JsonArray() { "Hello world!" }
            );
            var e = await response.GetThrownException<Exception>();
            Assert.AreEqual("Duh", e.Message);
            JsonObject body = await response.ReadJsonBody<JsonObject>();
            JsonArray logs = body["logs"];
            
            Assert.AreEqual(1, logs.Count);
            JsonObject record = logs[0];
            
            Assert.AreEqual("info", record["level"].AsString);
            Assert.IsTrue(record.ContainsKey("time"));
            Assert.AreEqual("Hello world!", record["message"].AsString);
            Assert.IsTrue(record["context"].IsNull);
        }
    }
}