using NUnit.Framework;
using System;
using UnityEngine;
using Unisave.Serialization;
using LightJson;
using System.Collections.Generic;

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

            Assert.AreEqual(42, Serializer.FromJson<int>("42"));
            Assert.AreEqual(0, Serializer.FromJson<int>("0"));
            Assert.AreEqual(-5, Serializer.FromJson<int>("-5"));
        }

        [Test]
        public void ItSerializesStrings()
        {
            Assert.AreEqual("\"foo\"", Serializer.ToJson("foo").ToString());
            Assert.AreEqual("\"lorem\\nipsum\"", Serializer.ToJson("lorem\nipsum").ToString());

            Assert.AreEqual("foo", Serializer.FromJson<string>("\"foo\""));
            Assert.AreEqual("lorem\nipsum", Serializer.FromJson<string>("\"lorem\\nipsum\""));
        }

        [Test]
        public void ItSerializesEnums()
        {
            Assert.AreEqual("\"First=0\"", Serializer.ToJson(MyEnum.First).ToString());
            Assert.AreEqual("\"Foo=42\"", Serializer.ToJson(MyEnum.Foo).ToString());
            Assert.AreEqual("\"Bar=8\"", Serializer.ToJson(MyEnum.Bar).ToString());

            Assert.AreEqual(MyEnum.Foo, Serializer.FromJson<MyEnum>("\"Foo=42\""));
            Assert.AreEqual(MyEnum.Foo, Serializer.FromJson<MyEnum>("\"Whatever=42\"")); // integer part rulez
        }

        [Test]
        public void ItSerializesArraysAndLists()
        {
            Assert.AreEqual("[1,2,3]", Serializer.ToJson(new int[] {1, 2, 3}).ToString());
            Assert.AreEqual("[1,2,3]", Serializer.ToJson(new List<int>(new int[] {1, 2, 3})).ToString());

            Assert.AreEqual(new int[] {1, 2, 3}, Serializer.FromJson<int[]>("[1,2,3]"));
            Assert.AreEqual(new int[] { 1, 2, 3 }, Serializer.FromJson<List<int>>("[1,2,3]").ToArray());
        }

        [Test]
        public void ItSerializesVectors()
        {
            Assert.AreEqual(@"{""x"":1,""y"":2,""z"":3}", Serializer.ToJson(new Vector3(1, 2, 3)).ToString());
            Assert.AreEqual(new Vector3(1, 2, 3), Serializer.FromJson<Vector3>(@"{""x"":1.0,""y"":2.0,""z"":3.0}"));

            Assert.AreEqual(@"{""x"":1,""y"":2}", Serializer.ToJson(new Vector2(1, 2)).ToString());
            Assert.AreEqual(new Vector2(1, 2), Serializer.FromJson<Vector2>(@"{""x"":1.0,""y"":2.0}"));
        }

        [Test]
        public void ItSerializesJson()
        {
            Assert.AreEqual("{}", Serializer.ToJson(new JsonObject()).ToString());
            Assert.AreEqual("\"hello\"", Serializer.ToJson(new JsonValue("hello")).ToString());
            Assert.AreEqual("-5", Serializer.ToJson(new JsonValue(-5)).ToString());

            Assert.AreEqual(42, Serializer.FromJson<JsonObject>("{\"a\": 42}")["a"].AsInteger);
            Assert.AreEqual(0, Serializer.FromJson<JsonValue>("0").AsInteger);
            Assert.AreEqual("foo", Serializer.FromJson<JsonValue>("\"foo\"").AsString);

            Assert.AreEqual(42, Serializer.FromJson<Dictionary<string, JsonValue>>("{\"a\": 42}")["a"].AsInteger);
        }
    }
}
