using System;
using FrameworkTests.TestingUtils;
using LightJson;
using NUnit.Framework;
using Unisave.Entities;
using Unisave.Serialization;

namespace FrameworkTests.Serialization.Unisave
{
    [TestFixture]
    public class EntitySerializationTest
    {
        private class StubEntity : Entity
        {
            public string Foo { get; set; } = "bar";

            public string bar = "baz";
        }
        
        [Test]
        public void ItSerializesEntities()
        {
            var entity = new StubEntity();
            entity.Foo = "Foo value";
            entity.bar = "bar value";
            
            var serialized = Serializer.ToJson(entity).ToString();
            
            Assert.AreEqual(
                new JsonObject {
                    ["Foo"] = "Foo value",
                    ["bar"] = "bar value",
                    ["_id"] = JsonValue.Null,
                    ["_rev"] = JsonValue.Null,
                    ["CreatedAt"] = Serializer.ToJson(default(DateTime)).AsString,
                    ["UpdatedAt"] = Serializer.ToJson(default(DateTime)).AsString,
                    ["_key"] = JsonValue.Null,
                }.ToString(),
                serialized
            );
            
            var deserialized = Serializer.FromJsonString<StubEntity>(serialized);

            UAssert.AreJsonEqual(entity, deserialized);
            Assert.AreEqual(entity.Foo, deserialized.Foo);
            Assert.AreEqual(entity.bar, deserialized.bar);
        }

        [Test]
        public void MissingFieldOnDeserializationWillBeSetToDefault()
        {
            var e = Serializer.FromJsonString<StubEntity>(
                "{'_id':'e_StubEntity/some-id'}".Replace('\'', '"')
            );
            var def = new StubEntity();
            
            Assert.AreEqual("e_StubEntity/some-id", e.EntityId);
            Assert.AreEqual(def.Foo, e.Foo);
            Assert.AreEqual(def.bar, e.bar);
        }
    }
}