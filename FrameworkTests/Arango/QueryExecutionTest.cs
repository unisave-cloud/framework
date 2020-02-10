using LightJson;
using NUnit.Framework;
using Unisave.Arango.Emulation;
using Unisave.Arango.Execution;
using Unisave.Arango.Query;

namespace FrameworkTests.Arango
{
    [TestFixture]
    public class QueryExecutionTest
    {
        private QueryExecutor executor;
        private ArangoInMemory arango;
        
        [SetUp]
        public void SetUp()
        {
            arango = new ArangoInMemory();
            executor = arango.Executor;
        }
        
        [Test]
        public void TestReturn()
        {
            Assert.AreEqual(
                "[5]",
                executor.ExecuteToArray(
                    new AqlQuery()
                        .Return(() => 5)
                ).ToString()
            );
        }

        [Test]
        public void TestFor()
        {
            // overwrite the COLLECTION function
            arango.FunctionRepository
                .Register("COLLECTION", args => new JsonArray(
                    new JsonObject()
                        .Add("c", args[0]),
                    new JsonObject()
                        .Add("foo", "bar")
                ));
            
            Assert.AreEqual(
                "[{'c':'users'},{'foo':'bar'}]".Replace("'", "\""),
                executor.ExecuteToArray(
                    new AqlQuery()
                        .For("u").In("users").Do()
                        .Return((u) => u)
                ).ToString()
            );
            
            Assert.AreEqual(
                "[2,2]".Replace("'", "\""),
                executor.ExecuteToArray(
                    new AqlQuery()
                        .For("u").In("users").Do()
                        .Return(() => 2)
                ).ToString()
            );
        }
    }
}