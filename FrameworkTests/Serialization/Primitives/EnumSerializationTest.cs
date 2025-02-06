using System.Collections.Generic;
using LightJson;
using NUnit.Framework;
using Unisave.Serialization;

namespace FrameworkTests.Serialization.Primitives
{
    [TestFixture]
    public class EnumSerializationTest
    {
        enum MyEnum
        {
            First,
            Foo = 42,
            Bar = 8
        }
        
        [Test]
        public void ItDeserializesLegacyEnums()
        {
            Assert.AreEqual(
                MyEnum.Foo,
                Serializer.FromJsonString<MyEnum>("\"Foo=42\"")
            );
            
            Assert.AreEqual(
                MyEnum.Foo,
                Serializer.FromJsonString<MyEnum>("\"Whatever=42\"")
            );
        }

        [Test]
        public void ItSerializesEnums()
        {
            Assert.AreEqual("0", Serializer.ToJson(MyEnum.First).ToString());
            Assert.AreEqual("42", Serializer.ToJson(MyEnum.Foo).ToString());
            Assert.AreEqual("8", Serializer.ToJson(MyEnum.Bar).ToString());

            Assert.AreEqual(MyEnum.First, Serializer.FromJsonString<MyEnum>("0"));
            Assert.AreEqual(MyEnum.Foo, Serializer.FromJsonString<MyEnum>("42"));
            Assert.AreEqual(MyEnum.Bar, Serializer.FromJsonString<MyEnum>("8"));
        }

        [Test]
        public void ItDeserializesEnumsFromStrings()
        {
            // because this happens in dictionary keys
            
            Assert.AreEqual(MyEnum.First, Serializer.FromJsonString<MyEnum>("\"0\""));
            Assert.AreEqual(MyEnum.Foo, Serializer.FromJsonString<MyEnum>("\"42\""));
            Assert.AreEqual(MyEnum.Bar, Serializer.FromJsonString<MyEnum>("\"8\""));
        }

        [Test]
        public void ItSerializesEnumKeyDictionaries()
        {
            var subject = new Dictionary<MyEnum, string> {
                [MyEnum.Foo] = "foo",
                [MyEnum.Bar] = "bar"
            };

            Assert.AreEqual(
                "{'42':'foo','8':'bar'}".Replace('\'', '\"'),
                Serializer.ToJson(subject).ToString()
            );

            Assert.AreEqual(
                subject,
                Serializer.FromJsonString<Dictionary<MyEnum, string>>(
                    "{'42':'foo','8':'bar'}".Replace('\'', '\"')
                )
            );
        }

        [Test]
        public void ItDeserializesEnumsAsEnumTypeNotInt()
        {
            object o = Serializer.FromJson(new JsonValue(42), typeof(MyEnum));
            
            Assert.IsTrue(o is MyEnum);
            Assert.IsFalse(o is int);
            
            Assert.AreEqual(MyEnum.Foo, o);
        }
    }
}