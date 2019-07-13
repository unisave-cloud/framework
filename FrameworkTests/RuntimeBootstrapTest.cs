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
    public class RuntimeBootstrapTest
    {
        private JsonObject RunWithParams(string facetName, string methodName, JsonArray arguments)
        {
            var executionParameters = new JsonObject();
            executionParameters.Add("facetName", facetName);
            executionParameters.Add("methodName", methodName);
            executionParameters.Add("arguments", arguments);
            executionParameters.Add("callerId", "fake-caler-id");
            executionParameters.Add("executionId", "fake-execution-id");

            string result = Bootstrap.FacetCall(executionParameters.ToString(), new Type[] {
                typeof(FakeFacet),

                typeof(WrongFacet),
                typeof(RuntimeBootstrapTest),
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
            Assert.AreEqual((int)Bootstrap.ErrorType.FacetNotFound, result["errorType"].AsInteger);
        }

        [Test]
        public void ItChecksMethodExistance()
        {
            JsonObject result = RunWithParams("FakeFacet", "NonexistingMethod", new JsonArray());
            Assert.AreEqual((int)Bootstrap.ErrorType.MethodDoesNotExist, result["errorType"].AsInteger);
        }

        [Test]
        public void ItChecksAmbiguousMethods()
        {
            JsonObject result = RunWithParams("FakeFacet", "AmbiguousMethod", new JsonArray());
            Assert.AreEqual((int)Bootstrap.ErrorType.MethodNameAmbiguous, result["errorType"].AsInteger);
        }

        [Test]
        public void ItChecksPublicMethods()
        {
            JsonObject result = RunWithParams("FakeFacet", "PrivateProcedure", new JsonArray());
            Assert.AreEqual((int)Bootstrap.ErrorType.MethodNotPublic, result["errorType"].AsInteger);
        }

        [Test]
        public void ItRunsFunctions()
        {
            JsonObject result = RunWithParams("FakeFacet", "SquaringFunction", new JsonArray().Add(5));
            Assert.AreEqual("ok", result["result"].AsString);
            Assert.IsTrue(result["hasReturnValue"]);
            Assert.AreEqual(25, result["returnValue"].AsInteger);
        }

        [Test]
        public void ItHandlesExceptions()
        {
            JsonObject result = RunWithParams("FakeFacet", "ExceptionalMethod", new JsonArray());
            Assert.AreEqual("game-exception", result["result"].AsString);
        }
    }
}
