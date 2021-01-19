using System.Collections.Generic;
using NUnit.Framework;
using Unisave.Serialization;

namespace FrameworkTests.Serialization.Collections
{
    [TestFixture]
    public class ListSerializationTest
    {
        [Test]
        public void ItSerializesLists()
        {
            Assert.AreEqual(
                "[1,2,3]",
                Serializer.ToJsonString(new List<int> {1, 2, 3})
            );
            
            Assert.AreEqual(
                "['a','b','c',null]".Replace('\'', '"'),
                Serializer.ToJsonString(new List<string> {"a", "b", "c", null})
            );
        }

        [Test]
        public void ItDeserializesLists()
        {
            Assert.AreEqual(
                new List<int> {1, 2, 3},
                Serializer.FromJsonString<List<int>>("[1,2,3]")
            );
            
            Assert.AreEqual(
                new List<string> {"a", "b", "c", null},
                Serializer.FromJsonString<List<string>>(
                    "['a','b','c',null]".Replace('\'', '"')
                )
            );
        }
    }
}