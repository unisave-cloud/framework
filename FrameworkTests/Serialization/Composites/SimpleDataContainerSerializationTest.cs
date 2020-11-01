using LightJson;
using NUnit.Framework;
using Unisave.Serialization;

namespace FrameworkTests.Serialization.Composites
{
    [TestFixture]
    public class SimpleDataContainerSerializationTest
    {
        public class MyContainer
        {
            public string foo;
            public MySubContainer bar;
            public string Baz { get; set; }
        }

        public class MySubContainer
        {
            public int a;
            public string b;
        }

        [Test]
        public void ItSerializesSimpleDataContainer()
        {
            var value = new MyContainer {
                foo = "foo",
                bar = new MySubContainer {
                    a = 42,
                    b = "b"
                },
                Baz = "baz"
            };
            
            var expectedJson = new JsonObject {
                ["foo"] = "foo",
                ["bar"] = new JsonObject {
                    ["a"] = 42,
                    ["b"] = "b"
                },
                ["Baz"] = "baz"
            };
            
            Assert.AreEqual(
                expectedJson.ToString(),
                Serializer.ToJsonString(value)
            );
        }

        [Test]
        public void ItDeserializesSimpleDataContainer()
        {
            var json = new JsonObject {
                ["foo"] = "foo",
                ["bar"] = new JsonObject {
                    ["a"] = 42,
                    ["b"] = "b"
                },
                ["Baz"] = "baz"
            };
            
            var value = Serializer.FromJson<MyContainer>(json);
            
            Assert.IsInstanceOf<MyContainer>(value);
            Assert.AreEqual("foo", value.foo);
            Assert.AreEqual(42, value.bar.a);
            Assert.AreEqual("b", value.bar.b);
            Assert.AreEqual("baz", value.Baz);
        }
    }
}