using System;
using LightJson;
using NUnit.Framework;
using Unisave.Contracts;
using Unisave.Foundation;
using Unisave.Logging;
using Unisave.Providers;

namespace FrameworkTests.Logging
{
    [TestFixture]
    public class HookIntoUnityEngineTest
    {
        private Application app;
        private LogServiceProvider provider;
        
        [SetUp]
        public void SetUp()
        {
            // reset adapter to null
            UnityEngine.Debug.UnisaveAdapter = null;
            
            // create app and log service provider
            app = new Application(new Type[] { });
            provider = new LogServiceProvider(app);
        }

        [Test]
        public void ItHooksIntoUnityDebug()
        {
            Assert.IsNull(UnityEngine.Debug.UnisaveAdapter);
            
            provider.Register();
            
            Assert.NotNull(UnityEngine.Debug.UnisaveAdapter);
            
            // try logging and pull out the log
            
            UnityEngine.Debug.Log("Hello!");
            UnityEngine.Debug.LogWarning("B");
            UnityEngine.Debug.LogError("C");

            InMemoryLog log = (InMemoryLog) app.Resolve<ILog>();

            JsonArray records = log.ExportLog();
            
            Assert.AreEqual(3, records.Count);
            Assert.AreEqual("Hello!", records[0]["message"].AsString);
            Assert.AreEqual("B", records[1]["message"].AsString);
            Assert.AreEqual("C", records[2]["message"].AsString);
        }
    }
}