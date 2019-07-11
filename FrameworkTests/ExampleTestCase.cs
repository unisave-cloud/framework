using NUnit.Framework;
using System;
using Unisave;
using LightJson;

namespace FrameworkTests
{
    [TestFixture()]
    public class ExampleTestCase
    {
        [Test()]
        public void TestCase()
        {
            UnisavePlayer p = new UnisavePlayer("foo");

            Assert.AreEqual("foo", p.Id);

            var json = new JsonObject()
                .Add("bar", 42)
                .ToString();

            Assert.AreEqual(@"{""bar"":42}", json);
        }
    }
}
