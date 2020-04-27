using System.Collections.Generic;
using NUnit.Framework;
using Unisave.Serialization;

namespace FrameworkTests.Serialization
{
    [TestFixture]
    public class CollectionSerializationTest
    {
        [Test]
        public void ItSerializesArraysAndLists()
        {
            Assert.AreEqual(
                "[1,2,3]",
                Serializer.ToJson(new int[] {1, 2, 3}).ToString()
            );
            Assert.AreEqual(
                "[1,2,3]",
                Serializer.ToJson(new List<int>(new int[] {1, 2, 3})).ToString()
            );

            Assert.AreEqual(
                new int[] {1, 2, 3},
                Serializer.FromJsonString<int[]>("[1,2,3]")
            );
            Assert.AreEqual(
                new int[] { 1, 2, 3 },
                Serializer.FromJsonString<List<int>>("[1,2,3]").ToArray()
            );
        }

        [Test]
        public void ItSerializesDictionaries()
        {
            var subject = new Dictionary<string, int> {
                ["lorem"] = 42,
                ["ipsum"] = 2
            };

            Assert.AreEqual(
                "{\"lorem\":42,\"ipsum\":2}",
                Serializer.ToJson(subject).ToString()
            );

            Assert.AreEqual(
                subject,
                Serializer.FromJsonString<Dictionary<string, int>>(
                    "{\"lorem\":42,\"ipsum\":2}"
                )
            );
        }

        [Test]
        public void ItSerializesNonStringDictionaries()
        {
            var subject = new Dictionary<int, string> {
                [5] = "42",
                [6] = "2"
            };

            Assert.AreEqual(
                "[[5,\"42\"],[6,\"2\"]]",
                Serializer.ToJson(subject).ToString()
            );

            Assert.AreEqual(
                subject,
                Serializer.FromJsonString<Dictionary<int, string>>(
                    "[[5,\"42\"],[6,\"2\"]]"
                )
            );
        }
    }
}