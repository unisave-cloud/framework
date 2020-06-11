using FrameworkTests.TestingUtils;
using NUnit.Framework;
using Unisave.Entities;
using Unisave.Serialization;

namespace FrameworkTests.Serialization
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