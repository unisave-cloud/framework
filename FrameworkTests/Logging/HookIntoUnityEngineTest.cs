using FrameworkTests.Testing;
using LightJson;
using NUnit.Framework;
using Unisave.Contracts;
using Unisave.Logging;

namespace FrameworkTests.Logging
{
    [TestFixture]
    public class HookIntoUnityEngineTest : RequestContextFixture
    {
        private InMemoryLog log;
        
        [SetUp]
        public void SetUp()
        {
            // reset adapter to null
            UnityEngine.Debug.UnisaveAdapter = null;

            // set the logger
            log = new InMemoryLog();
            ctx.Services.RegisterInstance<ILog>(log);
        }

        [Test]
        public void ItHooksIntoUnityDebug()
        {
            Assert.IsNull(UnityEngine.Debug.UnisaveAdapter);
            
            LoggingBootstrapper.HookIntoUnityEngineDebug();
            
            Assert.NotNull(UnityEngine.Debug.UnisaveAdapter);
            
            // try logging and pull out the log
            
            UnityEngine.Debug.Log("Hello!");
            UnityEngine.Debug.LogWarning("B");
            UnityEngine.Debug.LogError("C");

            JsonArray records = log.ExportLog();
            
            Assert.AreEqual(3, records.Count);
            Assert.AreEqual("Hello!", records[0]["message"].AsString);
            Assert.AreEqual("B", records[1]["message"].AsString);
            Assert.AreEqual("C", records[2]["message"].AsString);
        }
    }
}