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
    }
}