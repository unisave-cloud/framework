using NUnit.Framework;
using Unisave.Arango.Query;

namespace FrameworkTests.Arango
{
    [TestFixture]
    public class QueryToAqlTest
    {
        [Test]
        public void TestReturnOperator()
        {
            Assert.AreEqual(
                "RETURN 5",
                new AqlQuery()
                    .Return(() => 5)
                    .ToAql()
            );
        }
    }
}