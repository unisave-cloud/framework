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

        [Test]
        public void TestInsertOperator()
        {
            Assert.AreEqual(
                "FOR u IN users\n" +
                "INSERT u INTO backup\n" +
                "RETURN NEW",
                new AqlQuery()
                    .For("u").In("users").Do()
                    .Insert((u) => u).Into("backup")
                    .Return((NEW) => NEW)
                    .ToAql()
            );
            
            Assert.AreEqual(
                ("FOR u IN users\n" +
                "INSERT u INTO backup OPTIONS {'ignoreErrors':true}\n" +
                "RETURN NEW").Replace("'", "\""),
                new AqlQuery()
                    .For("u").In("users").Do()
                    .Insert((u) => u).IgnoreErrors().Into("backup")
                    .Return("NEW")
                    .ToAql()
            );
        }

        [Test]
        public void TestFilterOperator()
        {
            Assert.AreEqual(
                "FOR u IN users\n" +
                "FILTER (u.name == \"John\")\n" +
                "RETURN u",
                new AqlQuery()
                    .For("u").In("users").Do()
                    .Filter(u => u["name"] == "John")
                    .Return("u")
                    .ToAql()
            );
        }
    }
}