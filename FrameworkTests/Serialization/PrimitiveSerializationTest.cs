using System;
using System.Collections.Generic;
using LightJson;
using NUnit.Framework;
using Unisave.Serialization;

namespace FrameworkTests.Serialization
{
    [TestFixture]
    public class PrimitiveSerializationTest
    {
        [Test]
        public void ItSerializesIntegers()
        {
            Assert.AreEqual("42", Serializer.ToJson(42).ToString());
            Assert.AreEqual("0", Serializer.ToJson(0).ToString());
            Assert.AreEqual("-5", Serializer.ToJson(-5).ToString());

            Assert.AreEqual(42, Serializer.FromJsonString<int>("42"));
            Assert.AreEqual(0, Serializer.FromJsonString<int>("0"));
            Assert.AreEqual(-5, Serializer.FromJsonString<int>("-5"));
        }

        [Test]
        public void ItSerializesStrings()
        {
            Assert.AreEqual(
                "\"foo\"",
                Serializer.ToJson("foo").ToString()
            );
            Assert.AreEqual(
                "\"lorem\\nipsum\"",
                Serializer.ToJson("lorem\nipsum").ToString()
            );

            Assert.AreEqual(
                "foo",
                Serializer.FromJsonString<string>("\"foo\"")
            );
            Assert.AreEqual(
                "lorem\nipsum",
                Serializer.FromJsonString<string>("\"lorem\\nipsum\"")
            );
        }

        [Test]
        public void ItSerializesDateTime()
        {
            var subject = DateTime.Now;
            var serialized = subject.ToString(SerializationParams.DateTimeFormat);

            Assert.AreEqual(
                serialized,
                Serializer.ToJson(subject).AsString
            );

            var loaded = Serializer.FromJson<DateTime>((JsonValue)serialized);
            Assert.AreEqual(subject.Year, loaded.Year);
            Assert.AreEqual(subject.Month, loaded.Month);
            Assert.AreEqual(subject.Day, loaded.Day);
            Assert.AreEqual(subject.Hour, loaded.Hour);
            Assert.AreEqual(subject.Minute, loaded.Minute);
            Assert.AreEqual(subject.Second, loaded.Second);
            Assert.AreEqual(0, loaded.Millisecond);
        }

        [Test]
        public void ItSerializesJson()
        {
            Assert.AreEqual(
                "{}",
                Serializer.ToJson(new JsonObject()).ToString()
            );
            Assert.AreEqual(
                "\"hello\"",
                Serializer.ToJson(new JsonValue("hello")).ToString()
            );
            Assert.AreEqual(
                "-5",
                Serializer.ToJson(new JsonValue(-5)).ToString()
            );

            Assert.AreEqual(
                42,
                Serializer.FromJsonString<JsonObject>("{\"a\": 42}")["a"].AsInteger
            );
            Assert.AreEqual(
                0,
                Serializer.FromJsonString<JsonValue>("0").AsInteger
            );
            Assert.AreEqual(
                "foo",
                Serializer.FromJsonString<JsonValue>("\"foo\"").AsString
            );

            Assert.AreEqual(
                42,
                Serializer.FromJsonString<Dictionary<string, JsonValue>>(
                    "{\"a\": 42}"
                )["a"].AsInteger
            );
        }
    }
}
