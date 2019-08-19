using NUnit.Framework;
using System;
using Unisave.Runtime;
using Unisave;
using LightJson;
using LightJson.Serialization;

namespace FrameworkTests
{
    public class FakeHook : PlayerRegistrationHook
    {
        public static string flag;

        public void Run(string f)
        {
            flag = f + Player.Id;
        }
    }

    [TestFixture]
    public class PlayerRegistrationHookCallTest
    {
        private JsonObject RunWithParams(JsonArray arguments)
        {
            var methodParameters = new JsonObject();
            methodParameters.Add("arguments", arguments);
            methodParameters.Add("playerId", "fake-player-id");

            var executionParameters = new JsonObject();
            executionParameters.Add("executionId", "fake-execution-id");
            executionParameters.Add("databaseProxyIp", JsonValue.Null);
            executionParameters.Add("databaseProxyPort", 0);
            executionParameters.Add("executionMethod", "player-registration-hook");
            executionParameters.Add("methodParameters", methodParameters);

            string result = Entrypoint.Start(executionParameters.ToString(), new Type[] {
                typeof(FakeHook),

                typeof(WrongFacet),
                typeof(FacetCallTest),
                typeof(System.Collections.Hashtable)
            });

            return JsonReader.Parse(result);
        }

        [Test]
        public void ItExecutesTheHook()
        {
            FakeHook.flag = null;
            JsonObject result = RunWithParams(new JsonArray().Add("hello"));
            Assert.AreEqual("hellofake-player-id", FakeHook.flag);
        }
    }
}
