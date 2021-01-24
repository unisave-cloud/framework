using System.Collections.Generic;
using NUnit.Framework;
using Unisave.Serialization;
using UnityEngine;

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
        public void ItDeserializesEmptyDictionariesFromPhpMangledObjects()
        {
            // MOTIVATION:
            // It could happen that JSON gets mangled up by PHP somewhere
            // (i miss it) and it turns {} to [],
            // but the dict should still load ok (as a failsafe).

            var dict = Serializer.FromJsonString<Dictionary<string, int>>("[]");
            
            Assert.NotNull(dict);
            Assert.IsEmpty(dict);
        }
        
        [Test]
        public void ItSerializesPrimitiveKeyDictionaries()
        {
            var subject = new Dictionary<int, string> {
                [5] = "foo",
                [6] = "bar"
            };

            Assert.AreEqual(
                "{'5':'foo','6':'bar'}".Replace('\'', '\"'),
                Serializer.ToJson(subject).ToString()
            );

            Assert.AreEqual(
                subject,
                Serializer.FromJsonString<Dictionary<int, string>>(
                    "{'5':'foo','6':'bar'}".Replace('\'', '\"')
                )
            );
        }

        [Test]
        public void ItSerializesNonStringDictionaries()
        {
            var subject = new Dictionary<Vector2, string> {
                [Vector2.zero] = "foo",
                [Vector2.one] = "bar"
            };

            Assert.AreEqual(
                "[[{'x':0,'y':0},'foo'],[{'x':1,'y':1},'bar']]"
                    .Replace('\'', '\"'),
                Serializer.ToJson(subject).ToString()
            );

            Assert.AreEqual(
                subject,
                Serializer.FromJsonString<Dictionary<Vector2, string>>(
                    "[[{'x':0,'y':0},'foo'],[{'x':1,'y':1},'bar']]"
                        .Replace('\'', '\"')
                )
            );
        }
        
        [Test]
        public void ItSerializesEmptyNonStringDictionaries()
        {
            var subject = new Dictionary<Vector2, string>();

            Assert.AreEqual(
                "[]",
                Serializer.ToJson(subject).ToString()
            );

            Assert.AreEqual(
                subject,
                Serializer.FromJsonString<Dictionary<Vector2, string>>(
                    "[]"
                )
            );
        }
    }
}