using System;
using LightJson;
using NUnit.Framework;
using Unisave.Entities;
using Unisave.Serialization;

namespace FrameworkTests.Entities
{
    [TestFixture]
    public class EntityReferenceTest
    {
        // dummy entity to point references at
        private class PlayerEntity : Entity { }
        private class MotorbikeEntity : Entity { }

        // sample values to investigate in tests
        private EntityReference<PlayerEntity> nullReference;
        private EntityReference<PlayerEntity> otherNullReference;
        private EntityReference<PlayerEntity> johnReference;
        private EntityReference<PlayerEntity> peterReference;

        private PlayerEntity peter;
        private MotorbikeEntity motorbike;

        [SetUp]
        public void SetUp()
        {
            nullReference = Serializer.FromJson<EntityReference<PlayerEntity>>(
                JsonValue.Null
            );
            
            otherNullReference = Serializer.FromJson<EntityReference<PlayerEntity>>(
                JsonValue.Null
            );
            
            peterReference = Serializer.FromJson<EntityReference<PlayerEntity>>(
                new JsonValue("e_PlayerEntity/peter")
            );
            
            johnReference = Serializer.FromJson<EntityReference<PlayerEntity>>(
                new JsonValue("e_PlayerEntity/john")
            );
            
            peter = new PlayerEntity();
            peter.EntityId = peterReference.TargetId;
            
            motorbike = new MotorbikeEntity();
            motorbike.EntityId = "e_MotorbikeEntity/motorbike";
        }
        
        [Test]
        public void ItSupportsNullChecks()
        {
            Assert.IsTrue(nullReference == null);
            Assert.IsTrue(nullReference.IsNull);
            Assert.IsTrue(nullReference.TargetId == null);
            
            Assert.IsFalse(peterReference == null);
            Assert.IsFalse(peterReference.IsNull);
            Assert.IsFalse(peterReference.TargetId == null);
        }

        [Test]
        public void ItSupportsEqualityChecks()
        {
            Assert.IsTrue(nullReference == otherNullReference);
            Assert.IsFalse(peterReference == nullReference);
            
            // ReSharper disable once EqualExpressionComparison
            Assert.IsTrue(peterReference == peterReference);
            Assert.IsFalse(peterReference == johnReference);
            
            // compare to entity
            Assert.IsTrue(peterReference == peter);
            Assert.IsTrue(peter == peterReference);
            Assert.IsFalse(johnReference == peter);
            Assert.IsFalse(peter == johnReference);
            
            // compare to string
            // (cast has to be explicit)
            Assert.IsTrue(
                peterReference == (EntityReference<PlayerEntity>)peter.EntityId
            );
        }

        [Test]
        public void ReferencesCanBeAssigned()
        {
            // assign to null
            EntityReference<PlayerEntity> foo = null;
            Assert.IsTrue(foo.IsNull);

            // assign from other references
            // (and make sure it's IMMUTABLE! otherwise havoc unleashed)
            EntityReference<PlayerEntity> bar = peterReference;
            Assert.IsTrue(bar == peterReference);
            bar = johnReference;
            Assert.IsFalse(bar == peterReference);
            Assert.IsFalse(johnReference == peterReference); // check immutability

            // assign from entity instances
            EntityReference<PlayerEntity> baz = peter;
            Assert.IsTrue(baz == peterReference);

            // assign from string
            // (cast has to be explicit)
            EntityReference<PlayerEntity> asd
                = (EntityReference<PlayerEntity>)"e_PlayerEntity/some-id";
            Assert.AreEqual(asd.TargetId, "e_PlayerEntity/some-id");
        }

        [Test]
        public void ReferencesCheckGivenIdFormat()
        {
            // you cannot assign a non-id string to a reference
            Assert.Throws<ArgumentException>(() => {
                peterReference = (EntityReference<PlayerEntity>)"lorem ipsum";
            });
        }

        [Test]
        public void ReferencesCheckGivenType()
        {
            // you cannot assign type X into a reference to type Y
            Assert.Throws<ArgumentException>(() => {
                peterReference = (EntityReference<PlayerEntity>)motorbike.EntityId;
            });
        }
    }
}