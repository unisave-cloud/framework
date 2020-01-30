using NUnit.Framework;
using System;
using FrameworkTests.Facets;
using FrameworkTests.Facets.Stubs;
using Unisave.Runtime;
using Unisave;
using LightJson;
using LightJson.Serialization;
using Unisave.Exceptions;

namespace FrameworkTests
{
    public class FakeHook : PlayerRegistrationHook
    {
        public static string flag;

        public override void Run()
        {
            var argument = GetArgument<string>("foo");

            if (argument == "reject")
                Reject("Reject coz I want to.");

            flag = argument + Player.Id;
        }
    }

    [TestFixture]
    public class PlayerRegistrationHookCallTest
    {
        private JsonObject RunWithParams(JsonObject arguments)
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
            JsonObject result = RunWithParams(new JsonObject().Add("foo", "hello"));
            Assert.AreEqual("hellofake-player-id", FakeHook.flag);
        }

        [Test]
        public void HookThrows()
        {
            FakeHook.flag = null;
            JsonObject result = RunWithParams(new JsonObject().Add("foo", "reject"));
            Assert.AreEqual("exception", result["result"].AsString);
            Assert.AreEqual(null, FakeHook.flag);
        }
    }
}
