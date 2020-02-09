using LightJson;
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

        [Test]
        public void TestForOperator()
        {
            Assert.AreEqual(
                "FOR u IN users\n" +
                "RETURN u",
                new AqlQuery()
                    .For("u").In("users").Do()
                    .Return((u) => u)
                    .ToAql()
            );
            
            JsonArray c = new JsonArray(1, 2, 3);
            Assert.AreEqual(
                "FOR i IN [1,2,3]\n" +
                "RETURN i",
                new AqlQuery()
                    .For("i").In(() => c).Do()
                    .Return((i) => i)
                    .ToAql()
            );
        }
    }
}