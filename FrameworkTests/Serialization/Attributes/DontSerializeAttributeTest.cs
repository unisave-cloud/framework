using LightJson;
using NUnit.Framework;
using Unisave;
using Unisave.Serialization;

namespace FrameworkTests.Serialization.Attributes
{
    [TestFixture]
    public class DontSerializeAttributeTest
    {
        private class MyCustomType
        {
            // plain field
            
            public string fooYes;
            
            [DontSerialize]
            public string fooNo;
            
            // backing field
            
            public string BarYes { get; set; }
            
            [DontSerialize]
            public string BarNo { get; set; }
        }

        [Test]
        public void MarkedFieldsDontGetSerialized()
        {
            Assert.AreEqual(
                "{'fooYes':'fooYes','BarYes':'BarYes'}".Replace('\'', '"'),
                Serializer.ToJsonString(new MyCustomType {
                    fooYes = "fooYes",
                    fooNo = "fooNo",
                    BarYes = "BarYes",
                    BarNo = "BarNo"
                })
            );
        }

        [Test]
        public void MarkedFieldsDontGetLoadedEvenIfPresent()
        {
            var x = Serializer.FromJson<MyCustomType>(new JsonObject {
                ["fooYes"] = "fooYes",
                ["fooNo"] = "fooNo",
                ["BarYes"] = "BarYes",
                ["BarNo"] = "BarNo"
            });
            
            Assert.IsNotNull(x);
            Assert.AreEqual("fooYes", x.fooYes);
            Assert.AreEqual(null, x.fooNo);
            Assert.AreEqual("BarYes", x.BarYes);
            Assert.AreEqual(null, x.BarNo);
        }
    }
}