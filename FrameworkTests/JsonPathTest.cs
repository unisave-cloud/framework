using NUnit.Framework;
using Unisave.Database;

namespace FrameworkTests
{
    [TestFixture]
    public class JsonPathTest
    {
        private void TestPath(string path, string result = null)
        {
            result = result ?? path;
            Assert.AreEqual(result, JsonPath.Parse(path).ToString());
        }
        
        [Test]
        public void ItCanBeParsedAndSerialized()
        {
            TestPath("");
            TestPath("Foo");
            TestPath("42");
            TestPath("[42]");
            TestPath("foo[42]");
            TestPath("foo.bar");
            TestPath("foo.bar.baz");
            TestPath("foo[\"bar\"][\"baz\"]", "foo.bar.baz");
            TestPath("foo[\"bar\"].baz", "foo.bar.baz");
            TestPath("[\"bar\"].baz", "bar.baz");
            TestPath("[\"Q:\\\"\"][\"baz\"]", "[\"Q:\\\"\"].baz");
        }
    }
}