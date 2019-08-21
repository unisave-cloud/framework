using NUnit.Framework;
using System;
using UnityEngine;
using Unisave.Serialization;
using LightJson;
using System.Collections.Generic;

namespace FrameworkTests
{
    [TestFixture]
    public class SerializerTest
    {
        ////////////////
        // Primitives //
        ////////////////

        [Test]
        public void ItSerializesIntegers()
        {
            Assert.AreEqual("42", Saver.Save(42).ToString());
            Assert.AreEqual("0", Saver.Save(0).ToString());
            Assert.AreEqual("-5", Saver.Save(-5).ToString());

            Assert.AreEqual(42, Loader.Load<int>("42"));
            Assert.AreEqual(0, Loader.Load<int>("0"));
            Assert.AreEqual(-5, Loader.Load<int>("-5"));
        }

        [Test]
        public void ItSerializesStrings()
        {
            Assert.AreEqual("\"foo\"", Saver.Save("foo").ToString());
            Assert.AreEqual("\"lorem\\nipsum\"", Saver.Save("lorem\nipsum").ToString());

            Assert.AreEqual("foo", Loader.Load<string>("\"foo\""));
            Assert.AreEqual("lorem\nipsum", Loader.Load<string>("\"lorem\\nipsum\""));
        }

        //////////////////////
        // Unity Primitives //
        //////////////////////

        [Test]
        [Ignore("Serialization uses native code of Unity, needs to be fixed in the future.")]
        public void ItSerializesVectors()
        {
            Assert.AreEqual(@"{""x"":1,""y"":2,""z"":3}", Saver.Save(new Vector3(1, 2, 3)).ToString());
            Assert.AreEqual(new Vector3(1, 2, 3), Loader.Load<Vector3>(@"{""x"":1.0,""y"":2.0,""z"":3.0}"));

            Assert.AreEqual(@"{""x"":1,""y"":2}", Saver.Save(new Vector2(1, 2)).ToString());
            Assert.AreEqual(new Vector2(1, 2), Loader.Load<Vector2>(@"{""x"":1.0,""y"":2.0}"));
        }

        //////////
        // Json //
        //////////

        [Test]
        public void ItSerializesJson()
        {
            Assert.AreEqual("{}", Saver.Save(new JsonObject()).ToString());
            Assert.AreEqual("\"hello\"", Saver.Save(new JsonValue("hello")).ToString());
            Assert.AreEqual("-5", Saver.Save(new JsonValue(-5)).ToString());

            Assert.AreEqual(42, Loader.Load<JsonObject>("{\"a\": 42}")["a"].AsInteger);
            Assert.AreEqual(0, Loader.Load<JsonValue>("0").AsInteger);
            Assert.AreEqual("foo", Loader.Load<JsonValue>("\"foo\"").AsString);

            Assert.AreEqual(42, Loader.Load<Dictionary<string, JsonValue>>("{\"a\": 42}")["a"].AsInteger);
        }
    }
}
