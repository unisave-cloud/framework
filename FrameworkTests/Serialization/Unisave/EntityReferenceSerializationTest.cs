using LightJson;
using NUnit.Framework;
using Unisave.Entities;
using Unisave.Serialization;

namespace FrameworkTests.Serialization.Unisave
{
    [TestFixture]
    public class EntityReferenceSerializationTest
    {
        private class StubEntity : Entity { }

        [Test]
        public void ItSerializesReferences()
        {
            var reference = new EntityReference<StubEntity>("e_StubEntity/foo");
            Assert.AreEqual(
                "\"e_StubEntity\\/foo\"",
                Serializer.ToJsonString(reference)
            );

            reference = null;
            Assert.AreEqual(
                "null",
                Serializer.ToJsonString(reference)
            );
        }

        [Test]
        public void ItDeserializesReferences()
        {
            var deserializedRef = Serializer.FromJsonString<EntityReference<StubEntity>>(
                "\"e_StubEntity\\/foo\""
            );
            Assert.AreEqual(
                "e_StubEntity/foo",
                deserializedRef.TargetId
            );
            
            var deserializedNull = Serializer.FromJsonString<EntityReference<StubEntity>>(
                "null"
            );
            Assert.IsNull(deserializedNull.TargetId);
        }
    }
}