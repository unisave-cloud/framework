using System.Collections.Generic;
using NUnit.Framework;
using Unisave.Serialization;

namespace FrameworkTests.Serialization.Collections
{
    [TestFixture]
    public class DictionarySerializationTest
    {
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
        public void ItSerializesEmptyDictionaries()
        {
            var subject = new Dictionary<string, int>();

            Assert.AreEqual(
                "{}",
                Serializer.ToJson(subject).ToString()
            );

            Assert.AreEqual(
                subject,
                Serializer.FromJsonString<Dictionary<string, int>>(
                    "{}"
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
        
        [Test]
        public void ItSerializesEmptyNonStringDictionaries()
        {
            var subject = new Dictionary<int, string>();

            Assert.AreEqual(
                "[]",
                Serializer.ToJson(subject).ToString()
            );

            Assert.AreEqual(
                subject,
                Serializer.FromJsonString<Dictionary<int, string>>(
                    "[]"
                )
            );
        }
    }
}