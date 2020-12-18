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
        }
        
        [Test]
        public void ItSupportsNullChecks()
        {
            //Assert.IsTrue(nullReference == null); // FAILS!
            Assert.IsTrue(nullReference.IsNull);
            Assert.IsTrue(nullReference.TargetId == null);
            
            Assert.IsFalse(peterReference == null);
            Assert.IsFalse(peterReference.IsNull);
            Assert.IsFalse(peterReference.TargetId == null);
        }

        [Test]
        public void ItSupportsEqualityChecks()
        {
            // one reference variable equals another reference
        }

        [Test]
        public void ReferencesCanBeAssigned()
        {
            // assign to null
            
            // assign from other references
            // (and make sure it's IMMUTABLE! otherwise havoc unleashed)
            
            // assign from entity instances
            
            // assign from string
        }

        [Test]
        public void ReferencesCheckGivenType()
        {
            // you cannot assign type X into a reference to type Y
        }
    }
}