using System;
using LightJson;
using NUnit.Framework;
using Unisave.Serialization;
using UnityEngine;

namespace FrameworkTests.Serialization.Primitives
{
    [TestFixture]
    public class NullableSerializationTest
    {
        private class Composite
        {
            public int? foo;
            public bool? bar;
        }
        
        [Test]
        public void ItSerializesNullableInt()
        {
            Assert.AreEqual("42", Serializer.ToJsonString<int?>((int?)42));
            Assert.AreEqual("null", Serializer.ToJsonString<int?>((int?)null));
            
            Assert.AreEqual(42, Serializer.FromJsonString<int?>("42"));
            Assert.AreEqual(null, Serializer.FromJsonString<int?>("null"));
        }
        
        [Test]
        public void ItSerializesNullableBool()
        {
            Assert.AreEqual("true", Serializer.ToJsonString<bool?>((bool?)true));
            Assert.AreEqual("null", Serializer.ToJsonString<bool?>((bool?)null));
            
            Assert.AreEqual(true, Serializer.FromJsonString<bool?>("true"));
            Assert.AreEqual(null, Serializer.FromJsonString<bool?>("null"));
        }
        
        [Test]
        public void ItSerializesNullablesInComposites()
        {
            Assert.AreEqual(
                new JsonObject {
                    ["foo"] = 42,
                    ["bar"] = true
                }.ToString(),
                Serializer.ToJsonString(new Composite {
                    foo = 42,
                    bar = true
                })
            );
            
            Assert.AreEqual(
                new JsonObject {
                    ["foo"] = JsonValue.Null,
                    ["bar"] = JsonValue.Null
                }.ToString(),
                Serializer.ToJsonString(new Composite {
                    foo = null,
                    bar = null
                })
            );

            var c = Serializer.FromJson<Composite>(new JsonObject {
                ["foo"] = JsonValue.Null,
                ["bar"] = JsonValue.Null
            });
            Assert.AreEqual(null, c.foo);
            Assert.AreEqual(null, c.bar);
            
            c = Serializer.FromJson<Composite>(new JsonObject {
                ["foo"] = 42,
                ["bar"] = true
            });
            Assert.AreEqual(42, c.foo);
            Assert.AreEqual(true, c.bar);
        }
        
        [Test]
        public void ItSerializesNullableComposites()
        {
            Vector3 v = Vector3.forward;
            JsonValue vJson = Serializer.ToJson(v);
            
            Assert.AreEqual(
                vJson.ToString(),
                Serializer.ToJsonString((Vector3?) v)
            );
            
            Assert.AreEqual(
                "null",
                Serializer.ToJsonString((Vector3?) null)
            );
            
            Assert.AreEqual(
                v,
                Serializer.FromJson<Vector3?>(vJson)
            );
            
            Assert.AreEqual(
                null,
                Serializer.FromJson<Vector3?>(JsonValue.Null)
            );
        }
        
        [Test]
        public void ItSerializesNullableDateTime()
        {
            DateTime now = DateTime.Today;
            JsonValue nowJson = Serializer.ToJson(now);
            
            Assert.AreEqual(
                nowJson.ToString(),
                Serializer.ToJsonString((DateTime?) now)
            );
            
            Assert.AreEqual(
                "null",
                Serializer.ToJsonString((DateTime?) null)
            );
            
            Assert.AreEqual(
                now,
                Serializer.FromJson<DateTime?>(nowJson)
            );
            
            Assert.AreEqual(
                null,
                Serializer.FromJson<DateTime?>(JsonValue.Null)
            );
        }
    }
}