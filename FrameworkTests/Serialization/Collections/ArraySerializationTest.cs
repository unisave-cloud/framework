using NUnit.Framework;
using Unisave.Serialization;

namespace FrameworkTests.Serialization.Collections
{
    [TestFixture]
    public class ArraySerializationTest
    {
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
    }
}