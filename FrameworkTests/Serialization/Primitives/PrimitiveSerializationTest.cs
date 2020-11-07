using System.Collections.Generic;
using LightJson;
using NUnit.Framework;
using Unisave.Serialization;

namespace FrameworkTests.Serialization.Primitives
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
        public void ItSerializesAdvancedIntegers()
        {
            Assert.AreEqual("42", Serializer.ToJson((byte)42).ToString());
            Assert.AreEqual("0", Serializer.ToJson((byte)0).ToString());
            Assert.AreEqual(42, Serializer.FromJsonString<byte>("42"));
            Assert.AreEqual(0, Serializer.FromJsonString<byte>("0"));
            
            Assert.AreEqual("42", Serializer.ToJson((short)42).ToString());
            Assert.AreEqual("0", Serializer.ToJson((short)0).ToString());
            Assert.AreEqual("-5", Serializer.ToJson((short)-5).ToString());
            Assert.AreEqual(42, Serializer.FromJsonString<short>("42"));
            Assert.AreEqual(0, Serializer.FromJsonString<short>("0"));
            Assert.AreEqual(-5, Serializer.FromJsonString<short>("-5"));
            Assert.AreEqual(
                short.MaxValue,
                Serializer.FromJsonString<short>(short.MaxValue.ToString())
            );
            Assert.AreEqual(
                short.MinValue,
                Serializer.FromJsonString<short>(short.MinValue.ToString())
            );
            
            Assert.AreEqual("42", Serializer.ToJson((ushort)42).ToString());
            Assert.AreEqual("0", Serializer.ToJson((ushort)0).ToString());
            Assert.AreEqual(42, Serializer.FromJsonString<ushort>("42"));
            Assert.AreEqual(0, Serializer.FromJsonString<ushort>("0"));
            Assert.AreEqual(
                ushort.MaxValue,
                Serializer.FromJsonString<ushort>(ushort.MaxValue.ToString())
            );
            
            Assert.AreEqual("\"42\"", Serializer.ToJson((long)42).ToString());
            Assert.AreEqual("\"0\"", Serializer.ToJson((long)0).ToString());
            Assert.AreEqual("\"-5\"", Serializer.ToJson((long)-5).ToString());
            Assert.AreEqual(42, Serializer.FromJsonString<long>("\"42\""));
            Assert.AreEqual(0, Serializer.FromJsonString<long>("\"0\""));
            Assert.AreEqual(-5, Serializer.FromJsonString<long>("\"-5\""));
            Assert.AreEqual(
                long.MaxValue,
                Serializer.FromJsonString<long>("\"" + long.MaxValue + "\"")
            );
            Assert.AreEqual(
                long.MinValue,
                Serializer.FromJsonString<long>("\"" + long.MinValue + "\"")
            );
            
            Assert.AreEqual("\"42\"", Serializer.ToJson((ulong)42).ToString());
            Assert.AreEqual("\"0\"", Serializer.ToJson((ulong)0).ToString());
            Assert.AreEqual(42, Serializer.FromJsonString<ulong>("\"42\""));
            Assert.AreEqual(0, Serializer.FromJsonString<ulong>("\"0\""));
            Assert.AreEqual(
                ulong.MaxValue,
                Serializer.FromJsonString<ulong>("\"" + ulong.MaxValue + "\"")
            );
            
            Assert.AreEqual("\"42\"", Serializer.ToJson((uint)42).ToString());
            Assert.AreEqual("\"0\"", Serializer.ToJson((uint)0).ToString());
            Assert.AreEqual(42, Serializer.FromJsonString<uint>("\"42\""));
            Assert.AreEqual(0, Serializer.FromJsonString<uint>("\"0\""));
            Assert.AreEqual(
                uint.MaxValue,
                Serializer.FromJsonString<uint>("\"" + uint.MaxValue + "\"")
            );
        }
        
        [Test]
        public void ItSerializesFloats()
        {
            Assert.AreEqual("42.25", Serializer.ToJson(42.25f).ToString());
            Assert.AreEqual("0", Serializer.ToJson(0f).ToString());
            Assert.AreEqual("0", Serializer.ToJson(0.0).ToString());
            Assert.AreEqual("-5.5", Serializer.ToJson(-5.5).ToString());

            Assert.AreEqual(42.25f, Serializer.FromJsonString<float>("42.25"));
            Assert.AreEqual(42.25, Serializer.FromJsonString<double>("42.25"));
            Assert.AreEqual(0.0f, Serializer.FromJsonString<float>("0"));
            Assert.AreEqual(0.0, Serializer.FromJsonString<double>("0"));
            Assert.AreEqual(-5.5f, Serializer.FromJsonString<float>("-5.5"));
            Assert.AreEqual(-5.5, Serializer.FromJsonString<double>("-5.5"));
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
        public void ItSerializesChars()
        {
            Assert.AreEqual(
                "\"f\"",
                Serializer.ToJson('f').ToString()
            );
            Assert.AreEqual(
                "\"l\"",
                Serializer.ToJson('l').ToString()
            );

            Assert.AreEqual(
                'f',
                Serializer.FromJsonString<char>("\"f\"")
            );
            Assert.AreEqual(
                'l',
                Serializer.FromJsonString<char>("\"lorem\"")
            );
            
            Assert.AreEqual(
                '\0',
                Serializer.FromJsonString<char>("\"\"")
            );
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
