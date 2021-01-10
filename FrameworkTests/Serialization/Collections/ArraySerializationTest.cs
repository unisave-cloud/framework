using NUnit.Framework;
using Unisave.Serialization;

namespace FrameworkTests.Serialization.Collections
{
    [TestFixture]
    public class ArraySerializationTest
    {
        private class MyType
        {
            public int foo;
            public string bar;
        }
        
        [Test]
        public void ItSerializesAnArray()
        {
            Assert.AreEqual(
                "[1,2,3]",
                Serializer.ToJsonString(new int[] {1, 2, 3})
            );
            
            Assert.AreEqual(
                "['a','b','c',null]".Replace('\'', '"'),
                Serializer.ToJsonString(new string[] {"a", "b", "c", null})
            );
        }

        [Test]
        public void ItDeserializesAnArray()
        {
            Assert.AreEqual(
                new int[] {1, 2, 3},
                Serializer.FromJsonString<int[]>("[1,2,3]")
            );
            
            Assert.AreEqual(
                new string[] {"a", "b", "c", null},
                Serializer.FromJsonString<string[]>(
                    "['a','b','c',null]".Replace('\'', '"')
                )
            );
        }

        [Test]
        public void ItSerializesAnArrayOfCustomTypes()
        {
            var data = new MyType[] {
                new MyType(),
                null,
                new MyType {
                    foo = 42,
                    bar = "lorem"
                }
            };
            
            var json = "[{'foo':0,'bar':null},null,{'foo':42,'bar':'lorem'}]"
                .Replace('\'', '"');
            
            Assert.AreEqual(json, Serializer.ToJsonString(data));
        }

        [Test]
        public void ItDeserializesAnArrayOfCustomTypes()
        {
            var json = "[{'foo':0,'bar':null},null,{'foo':42,'bar':'lorem'}]"
                .Replace('\'', '"');
            
            var data = Serializer.FromJsonString<MyType[]>(json);
            
            Assert.AreEqual(3, data.Length);
            
            Assert.AreEqual(0, data[0].foo);
            Assert.AreEqual(null, data[0].bar);
            
            Assert.IsNull(data[1]);
            
            Assert.AreEqual(42, data[2].foo);
            Assert.AreEqual("lorem", data[2].bar);
        }
    }
}