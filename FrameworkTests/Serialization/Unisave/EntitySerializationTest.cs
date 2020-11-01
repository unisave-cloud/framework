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
                    ["_key"] = JsonValue.Null,
                    ["_id"] = JsonValue.Null,
                    ["_rev"] = JsonValue.Null,
                    ["Foo"] = "Foo value",
                    ["CreatedAt"] = Serializer.ToJson(default(DateTime)).AsString,
                    ["UpdatedAt"] = Serializer.ToJson(default(DateTime)).AsString,
                    ["bar"] = "bar value",
                }.ToString(),
                serialized
            );
            
            var deserialized = Serializer.FromJsonString<StubEntity>(serialized);

            UAssert.AreJsonEqual(entity, deserialized);
            Assert.AreEqual(entity.Foo, deserialized.Foo);
            Assert.AreEqual(entity.bar, deserialized.bar);
        }
        
        [Test]
        public void ItSerializesEntityReferences()
        {
            string id = EntityUtils.CollectionPrefix + "Entity/foo";
            var reference = new EntityReference<Entity>(id);

            Assert.AreEqual(id, Serializer.ToJson(reference).AsString);
            
            Assert.AreEqual(
                id,
                Serializer.FromJson<EntityReference<Entity>>(id).TargetId
            );
        }
    }
}