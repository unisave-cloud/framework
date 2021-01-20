using LightJson;
using NUnit.Framework;
using Unisave;
using Unisave.Serialization;

namespace FrameworkTests.Serialization.Attributes
{
    [TestFixture]
    public class SerializeAsAttributeTest
    {
        private class MyCustomType
        {
            // plain field
            [SerializeAs("FOO")]
            public string foo;
            
            // backing field
            [SerializeAs("BAR")]
            public string Bar { get; set; }
        }
        
        [Test]
        public void MarkedFieldsGetSerializedUnderTheName()
        {
            Assert.AreEqual(
                "{'FOO':'foo','BAR':'Bar'}".Replace('\'', '"'),
                Serializer.ToJsonString(new MyCustomType {
                    foo = "foo",
                    Bar = "Bar",
                })
            );
        }
        
        [Test]
        public void MarkedFieldsGetLoadedProperly()
        {
            var x = Serializer.FromJson<MyCustomType>(new JsonObject {
                ["FOO"] = "foo",
                ["BAR"] = "Bar",
            });
            
            Assert.IsNotNull(x);
            Assert.AreEqual("foo", x.foo);
            Assert.AreEqual("Bar", x.Bar);
        }
    }
}