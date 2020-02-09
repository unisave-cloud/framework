using NUnit.Framework;
using Unisave.Arango.Database;
using Unisave.Arango.Query;

namespace FrameworkTests.Arango
{
    [TestFixture]
    public class QueryExecutionTest
    {
        private QueryExecutor executor;
        
        [SetUp]
        public void SetUp()
        {
            executor = new QueryExecutor();
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