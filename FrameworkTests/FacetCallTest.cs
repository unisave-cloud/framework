using NUnit.Framework;
using System;
using Unisave.Runtime;
using Unisave;
using LightJson;
using LightJson.Serialization;

namespace FrameworkTests
{
    class FakeFacet : Facet
    {
        public static bool flag;

        public void MyProcedure()
        {
            flag = true;
        }

        public static void MyProcedure(int foo) {} // static methods should not be included in the lookup

        public void MyParametrizedProcedure(bool setFlagTo)
        {
            flag = setFlagTo;
        }

        public void AmbiguousMethod() {}
        private void AmbiguousMethod(int foo) {}

        private void PrivateProcedure() {}

        public int SquaringFunction(int x)
        {
            return x * x;
        }

        public void ExceptionalMethod()
        {
            throw new Exception("Some interesting exception.");
        }
    }

    class WrongFacet // no parent
    {
        public void MyProcedure() {}
    }

    [TestFixture]
    public class FacetCallTest
    {
        private JsonObject RunWithParams(string facetName, string methodName, JsonArray arguments)
        {
            var methodParameters = new JsonObject();
            methodParameters.Add("facetName", facetName);
            methodParameters.Add("methodName", methodName);
            methodParameters.Add("arguments", arguments);
            methodParameters.Add("callerId", "fake-caler-id");

            var executionParameters = new JsonObject();
            executionParameters.Add("executionId", "fake-execution-id");
            executionParameters.Add("databaseProxyIp", JsonValue.Null);
            executionParameters.Add("databaseProxyPort", 0);
            executionParameters.Add("executionMethod", "facet");
            executionParameters.Add("methodParameters", methodParameters);
            
            string result = Entrypoint.Start(executionParameters.ToString(), new Type[] {
                typeof(FakeFacet),

                typeof(WrongFacet),
                typeof(FacetCallTest),
                typeof(System.Collections.Hashtable)
            });

            return JsonReader.Parse(result);
        }

        [Test]
        public void ItRunsMyProcedure()
        {
            FakeFacet.flag = false;

            JsonObject result = RunWithParams("FakeFacet", "MyProcedure", new JsonArray());

            Assert.IsTrue(FakeFacet.flag);
            Assert.AreEqual("ok", result["result"].AsString);
            Assert.IsFalse(result["hasReturnValue"]);
        }

        [Test]
        public void ItRunsMyParametrizedProcedure()
        {
            FakeFacet.flag = false;

            JsonObject result = RunWithParams("FakeFacet", "MyParametrizedProcedure", new JsonArray().Add(true));
            Assert.AreEqual("ok", result["result"].AsString);
            Assert.IsTrue(FakeFacet.flag);

            result = RunWithParams("FakeFacet", "MyParametrizedProcedure", new JsonArray().Add(false));
            Assert.AreEqual("ok", result["result"].AsString);
            Assert.IsFalse(FakeFacet.flag);
        }

        [Test]
        public void ItChecksParentFacet()
        {
            JsonObject result = RunWithParams("WrongFacet", "MyProcedure", new JsonArray());
            Assert.AreEqual("invalid-method-parameters", result["result"].AsString);
            StringAssert.Contains("Facet class wasn't found.", result["message"]);
        }

        [Test]
        public void ItChecksMethodExistance()
        {
            JsonObject result = RunWithParams("FakeFacet", "NonexistingMethod", new JsonArray());
            Assert.AreEqual("invalid-method-parameters", result["result"].AsString);
            StringAssert.Contains("doesn't have public method called", result["message"]);
        }

        [Test]
        public void ItChecksAmbiguousMethods()
        {
            JsonObject result = RunWithParams("FakeFacet", "AmbiguousMethod", new JsonArray());
            Assert.AreEqual("invalid-method-parameters", result["result"].AsString);
            StringAssert.Contains("has multiple methods called", result["message"]);
        }

        [Test]
        public void ItChecksPublicMethods()
        {
            JsonObject result = RunWithParams("FakeFacet", "PrivateProcedure", new JsonArray());
            Assert.AreEqual("invalid-method-parameters", result["result"].AsString);
            StringAssert.Contains("has to be public in order to be called", result["message"]);
        }

        [Test]
        public void ItRunsFunctions()
        {
            JsonObject result = RunWithParams("FakeFacet", "SquaringFunction", new JsonArray().Add(5));
            Assert.AreEqual("ok", result["result"].AsString);
            Assert.IsTrue(result["methodResponse"]["hasReturnValue"]);
            Assert.AreEqual(25, result["methodResponse"]["returnValue"].AsInteger);
        }

        [Test]
        public void ItHandlesExceptions()
        {
            JsonObject result = RunWithParams("FakeFacet", "ExceptionalMethod", new JsonArray());
            Assert.AreEqual("exception", result["result"].AsString);
        }
    }
}
