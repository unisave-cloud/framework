using NUnit.Framework;
using System;
using UnityEngine;
using Unisave.Serialization;
using LightJson;
using System.Collections.Generic;
using FrameworkTests.TestingUtils;
using Unisave;
using Unisave.Database;

namespace FrameworkTests
{
    enum MyEnum
    {
        First,
        Foo = 42,
        Bar = 8
    }

    [TestFixture]
    public class SerializerTest
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
            Assert.AreEqual("\"foo\"", Serializer.ToJson("foo").ToString());
            Assert.AreEqual("\"lorem\\nipsum\"", Serializer.ToJson("lorem\nipsum").ToString());

            Assert.AreEqual("foo", Serializer.FromJsonString<string>("\"foo\""));
            Assert.AreEqual("lorem\nipsum", Serializer.FromJsonString<string>("\"lorem\\nipsum\""));
        }

        [Test]
        public void ItSerializesEnums()
        {
            Assert.AreEqual("\"First=0\"", Serializer.ToJson(MyEnum.First).ToString());
            Assert.AreEqual("\"Foo=42\"", Serializer.ToJson(MyEnum.Foo).ToString());
            Assert.AreEqual("\"Bar=8\"", Serializer.ToJson(MyEnum.Bar).ToString());

            Assert.AreEqual(MyEnum.Foo, Serializer.FromJsonString<MyEnum>("\"Foo=42\""));
            Assert.AreEqual(MyEnum.Foo, Serializer.FromJsonString<MyEnum>("\"Whatever=42\"")); // integer part rulez
        }

        [Test]
        public void ItSerializesArraysAndLists()
        {
            Assert.AreEqual("[1,2,3]", Serializer.ToJson(new int[] {1, 2, 3}).ToString());
            Assert.AreEqual("[1,2,3]", Serializer.ToJson(new List<int>(new int[] {1, 2, 3})).ToString());

            Assert.AreEqual(new int[] {1, 2, 3}, Serializer.FromJsonString<int[]>("[1,2,3]"));
            Assert.AreEqual(new int[] { 1, 2, 3 }, Serializer.FromJsonString<List<int>>("[1,2,3]").ToArray());
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

            Assert.AreEqual(subject, Serializer.FromJsonString<Dictionary<string, int>>("{\"lorem\":42,\"ipsum\":2}"));
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

            Assert.AreEqual(subject, Serializer.FromJsonString<Dictionary<int, string>>("[[5,\"42\"],[6,\"2\"]]"));
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
        public void ItSerializesVectors()
        {
            Assert.AreEqual(@"{""x"":1,""y"":2,""z"":3}", Serializer.ToJson(new Vector3(1, 2, 3)).ToString());
            Assert.AreEqual(new Vector3(1, 2, 3), Serializer.FromJsonString<Vector3>(@"{""x"":1.0,""y"":2.0,""z"":3.0}"));

            Assert.AreEqual(@"{""x"":1,""y"":2}", Serializer.ToJson(new Vector2(1, 2)).ToString());
            Assert.AreEqual(new Vector2(1, 2), Serializer.FromJsonString<Vector2>(@"{""x"":1.0,""y"":2.0}"));
        }
        
        [Test]
        public void ItSerializesColors()
        {
            Assert.AreEqual(
                @"{""r"":0.5,""g"":0.25,""b"":0.75,""a"":1}",
                Serializer.ToJson(new Color(0.5f, 0.25f, 0.75f, 1f)).ToString()
            );
            Assert.AreEqual(
                new Color(0.5f, 0.25f, 0.75f, 1f),
                Serializer.FromJsonString<Color>(
                    @"{""r"":0.5,""g"":0.25,""b"":0.75,""a"":1}"
                )
            );
            
            Assert.AreEqual(
                @"{""r"":1,""g"":2,""b"":3,""a"":4}",
                Serializer.ToJson(new Color32(1, 2, 3, 4)).ToString()
            );
            Assert.AreEqual(
                new Color32(1, 2, 3, 4),
                Serializer.FromJsonString<Color32>(
                    @"{""r"":1,""g"":2,""b"":3,""a"":4}"
                )
            );
        }

        [Test]
        public void ItSerializesJson()
        {
            Assert.AreEqual("{}", Serializer.ToJson(new JsonObject()).ToString());
            Assert.AreEqual("\"hello\"", Serializer.ToJson(new JsonValue("hello")).ToString());
            Assert.AreEqual("-5", Serializer.ToJson(new JsonValue(-5)).ToString());

            Assert.AreEqual(42, Serializer.FromJsonString<JsonObject>("{\"a\": 42}")["a"].AsInteger);
            Assert.AreEqual(0, Serializer.FromJsonString<JsonValue>("0").AsInteger);
            Assert.AreEqual("foo", Serializer.FromJsonString<JsonValue>("\"foo\"").AsString);

            Assert.AreEqual(42, Serializer.FromJsonString<Dictionary<string, JsonValue>>("{\"a\": 42}")["a"].AsInteger);
        }

        [Test]
        public void ItSerializesExceptions()
        {
            Exception exception = null;

            try
            {
                throw new InvalidOperationException();
            }
            catch (Exception e)
            {
                Assert.AreEqual(
                    e.ToString(),
                    Serializer.FromJson(Serializer.ToJson(e), typeof(Exception)).ToString()
                );

                exception = e;
            }

            Assert.AreEqual(
                exception.ToString(),
                Serializer.FromJson(Serializer.ToJson(exception), typeof(Exception)).ToString()
            );
        }

        [Test]
        public void EntityOwnerIdsCanBeLoadedFromMalformedJson()
        {
            EntityOwnerIds.FromJson(new JsonArray("asd", "bsd"));
            EntityOwnerIds.FromJson(JsonValue.Null);
            EntityOwnerIds.FromJson(new JsonObject());
        }

        [Test]
        public void ItSerializesCustomTypes()
        {
            string serialized = Serializer.ToJson(new DerivedClassType ("a", "b") {
                publicField = "foo",
                PublicGetSetProperty = "bar",
                addedField = "baz"
            }).ToString();
            
            Console.WriteLine(serialized);

            string reserialized = Serializer.ToJson(
                Serializer.FromJsonString<DerivedClassType>(serialized)
            ).ToString();

            Assert.AreEqual(serialized, reserialized);
        }

        private class CustomClassType
        {
            public string publicField;
            public string PublicGetSetProperty { get; set; }
            private string privateField;
            
            public string PublicGetPrivateSetProperty { get; private set; }

            public CustomClassType(string a, string b)
            {
                privateField = a;
                PublicGetPrivateSetProperty = b;
            }
        }

        private class DerivedClassType : CustomClassType
        {
            public string addedField;

            public DerivedClassType(string a, string b) : base (a, b) { }
        }

        [Test]
        public void ItSerializesEntities()
        {
            var entity = new SomeEntity();
            
            var serialized = Serializer.ToJson(entity).ToString();
            var deserialized = Serializer.FromJsonString<SomeEntity>(serialized);

            UAssert.AreJsonEqual(entity, deserialized);
            Assert.AreEqual(entity.Foo, deserialized.Foo);
        }

        private class SomeEntity : Entity
        {
            [X] public string Foo { get; set; } = "bar";
        }
    }
}
