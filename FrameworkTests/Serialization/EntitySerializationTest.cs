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
        }
        
        [Test]
        public void ItSerializesEntities()
        {
            var entity = new StubEntity();
            
            var serialized = Serializer.ToJson(entity).ToString();
            var deserialized = Serializer.FromJsonString<StubEntity>(serialized);

            UAssert.AreJsonEqual(entity, deserialized);
            Assert.AreEqual(entity.Foo, deserialized.Foo);
        }
        
        [Test]
        public void ItSerializesEntityReferences()
        {
            string id = "entities_Entity/foo";
            var reference = new EntityReference<Entity>(id);

            Assert.AreEqual(id, Serializer.ToJson(reference).AsString);
            
            Assert.AreEqual(
                id,
                Serializer.FromJson<EntityReference<Entity>>(id).TargetId
            );
        }
    }
}