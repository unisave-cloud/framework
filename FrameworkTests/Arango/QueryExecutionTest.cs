using LightJson;
using NUnit.Framework;
using Unisave.Arango;
using Unisave.Arango.Emulation;
using Unisave.Arango.Execution;
using Unisave.Arango.Expressions;
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

        [Test]
        public void TestInsert()
        {
            arango.CreateCollection("stuff", CollectionType.Document);
            
            JsonObject obj = new JsonObject()
                .Add("_key", "key")
                .Add("foo", 42);
            
            Assert.AreEqual(
                "[42]".Replace("'", "\""),
                executor.ExecuteToArray(
                    new AqlQuery()
                        .Insert(obj).Into("stuff")
                        .Return((NEW) => NEW["foo"])
                ).ToString()
            );
            
            Assert.AreEqual(
                "[42]".Replace("'", "\""),
                executor.ExecuteToArray(
                    new AqlQuery()
                        .Return(() => AF.Document("stuff", "key")["foo"])
                ).ToString()
            );
        }

        [Test]
        public void TestFilter()
        {
            arango.CreateCollection("stuff", CollectionType.Document);
            
            executor.Execute(
                new AqlQuery()
                    .For("i").In(() => new JsonArray(1, 2, 3, 4, 5, 6)).Do()
                    .Insert((i) => new JsonObject().Add("i", i)).Into("stuff")
            );
            
            Assert.AreEqual(
                "[4,5,6]".Replace("'", "\""),
                executor.ExecuteToArray(
                    new AqlQuery()
                        .For("s").In("stuff").Do()
                        .Filter(s => s["i"] > 3)
                        .Return(s => s["i"])
                ).ToString()
            );
        }
    }
}