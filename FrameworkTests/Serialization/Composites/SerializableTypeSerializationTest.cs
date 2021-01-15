using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using NUnit.Framework;
using Unisave.Serialization;

namespace FrameworkTests.Serialization.Composites
{
    [Serializable]
    public class MySerializableType : ISerializable
    {
        public int foo;
        
        public string Bar { get; set; }
        
        public List<int> sequence = new List<int>();

        public MySerializableType() { }
        
        protected MySerializableType(
            SerializationInfo info,
            StreamingContext context
        )
        {
            foo = (int) info.GetValue("f", typeof(int));
            Bar = (string) info.GetValue("b", typeof(string));
            sequence = (List<int>) info.GetValue("s", typeof(List<int>));
        }
        
        public void GetObjectData(
            SerializationInfo info,
            StreamingContext context
        )
        {
            info.AddValue("f", foo);
            info.AddValue("b", Bar);
            info.AddValue("s", sequence);
        }
    }
    
    [TestFixture]
    public class SerializableTypeSerializationTest
    {
        [Test]
        public void ItSerializesViaTheInterfaceMethod()
        {
            var subject = new MySerializableType {
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

            var data = Serializer.FromJsonString<MySerializableType>(json);
            
            Assert.IsNotNull(data);
            Assert.AreEqual(42, data.foo);
            Assert.AreEqual("Hello!", data.Bar);
            
            Assert.AreEqual(3, data.sequence.Count);
            Assert.AreEqual(9, data.sequence[0]);
            Assert.AreEqual(8, data.sequence[1]);
            Assert.AreEqual(7, data.sequence[2]);
        }

        [Test]
        public void ItDoesntDeserializeWithPartialData()
        {
            /*
             * NOTE:
             * ISerializable is meant for .NET classes that want to be
             * serialized certain way. It is not ideal for fluent Unisave
             * data as it doesn't allow simple addition of fields
             * and data migration on deserialization.
             */
            
            var json = "{'f':42}"
                .Replace('\'', '\"');

            Assert.Throws<SerializationException>(() => {
                Serializer.FromJsonString<MySerializableType>(json);
            });
        }
    }
}