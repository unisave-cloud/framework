using System.Collections.Generic;
using LightJson;
using NUnit.Framework;
using Unisave.Serialization;
using Unisave.Serialization.Context;

namespace FrameworkTests.Serialization.Composites
{
    public class MyUnisaveSerializableType : IUnisaveSerializable
    {
        public int foo;
        
        public string Bar { get; set; }
        
        public List<int> sequence = new List<int>();

        public MyUnisaveSerializableType() { }
        
        protected MyUnisaveSerializableType(
            JsonValue json,
            DeserializationContext context
        )
        {
            foo = Serializer.FromJson<int>(json["f"], context);
            Bar = Serializer.FromJson<string>(json["b"], context);
            sequence = Serializer.FromJson<List<int>>(json["s"], context);
        }
        
        public JsonValue ToJson(SerializationContext context)
        {
            return new JsonObject {
                ["f"] = foo,
                ["b"] = Bar,
                ["s"] = Serializer.ToJson(sequence, context)
            };
        }
    }
    
    [TestFixture]
    public class UnisaveSerializableTypeSerializationTest
    {
        [Test]
        public void ItSerializesViaTheInterfaceMethod()
        {
            var subject = new MyUnisaveSerializableType {
                foo = 42,
                Bar = "Hello!"
            };
            subject.sequence.Add(9);
            subject.sequence.Add(8);
            subject.sequence.Add(7);

            var givenJson = Serializer.ToJsonString(subject);
            var expectedJson = "{'f':42,'b':'Hello!','s':[9,8,7]}"
                .Replace('\'', '\"');
            
            Assert.AreEqual(expectedJson, givenJson);
        }

        [Test]
        public void ItDeserializesViaTheInterfaceConstructor()
        {
            var json = "{'f':42,'b':'Hello!','s':[9,8,7]}"
                .Replace('\'', '\"');

            var data = Serializer.FromJsonString<MyUnisaveSerializableType>(json);
            
            Assert.IsNotNull(data);
            Assert.AreEqual(42, data.foo);
            Assert.AreEqual("Hello!", data.Bar);
            
            Assert.AreEqual(3, data.sequence.Count);
            Assert.AreEqual(9, data.sequence[0]);
            Assert.AreEqual(8, data.sequence[1]);
            Assert.AreEqual(7, data.sequence[2]);
        }

        [Test]
        public void ItDeserializesWithPartialData()
        {
            var json = "{'f':42}"
                .Replace('\'', '\"');

            var data = Serializer.FromJsonString<MyUnisaveSerializableType>(json);
            
            Assert.IsNotNull(data);
            Assert.AreEqual(42, data.foo);
            Assert.IsNull(data.Bar);
            Assert.IsNull(data.sequence);
        }
    }
}