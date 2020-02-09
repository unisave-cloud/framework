using LightJson;
using NUnit.Framework;
using Unisave.Arango.Database;
using Unisave.Arango.Query;

namespace FrameworkTests.Arango
{
    [TestFixture]
    public class QueryExecutionTest
    {
        private AqlFunctionRepository functionRepository;
        private QueryExecutor executor;
        
        [SetUp]
        public void SetUp()
        {
            functionRepository = new AqlFunctionRepository();
            functionRepository.RegisterFunctions();
            
            executor = new QueryExecutor(functionRepository);
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
            functionRepository.Register("COLLECTION", args => new JsonArray(
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