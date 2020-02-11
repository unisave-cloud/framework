using System;
using System.Linq;
using LightJson;
using NUnit.Framework;
using Unisave.Arango;
using Unisave.Arango.Emulation;
using Unisave.Entities;

namespace FrameworkTests.Entities
{
    [TestFixture]
    public class EntityManagerTest
    {
        private ArangoInMemory arango;
        private EntityManager manager;

        private Collection players;
        private Collection motorbikes;
        
        [SetUp]
        public void SetUp()
        {
            arango = new ArangoInMemory();
            manager = new EntityManager(arango);
            
            players = arango.CreateCollection(
                "entities_PlayerEntity",
                CollectionType.Document
            );
            
            motorbikes = arango.CreateCollection(
                "entities_MotorbikeEntity",
                CollectionType.Document
            );

            players.Add(@"{
                '_key': 'john',
                'Name': 'John Doe'
            }");
            
            players.Add(@"{
                '_key': 'peter',
                'Name': 'Peter'
            }");
            
            motorbikes.Add(@"{
                '_key': 'john',
                'Name': 'John-deer',
                'Model': 'latest'
            }");
        }
        
        [Test]
        public void ItFindsJohnThePlayer()
        {
            var john = manager.Find("PlayerEntity", "john");
            
            Assert.AreEqual("john", john["_key"].AsString);
            Assert.AreEqual("John Doe", john["Name"].AsString);
            Assert.AreEqual("PlayerEntity", john["_type"].AsString);
            
            Assert.AreEqual(null, john["Model"].AsString);
        }
        
        [Test]
        public void ItFindsJohnTheMotorbike()
        {
            var john = manager.Find("MotorbikeEntity", "john");
            
            Assert.AreEqual("john", john["_key"].AsString);
            Assert.AreEqual("John-deer", john["Name"].AsString);
            Assert.AreEqual("MotorbikeEntity", john["_type"].AsString);
        }

        [Test]
        public void ItFindsNull()
        {
            var peterTheMotorbike = manager.Find("MotorbikeEntity", "peter");
            Assert.IsNull(peterTheMotorbike);
        }

        [Test]
        public void ItFindsNullInNonExistingCollection()
        {
            var entity = manager.Find("NonExistingEntityType", "someKey");
            Assert.IsNull(entity);
        }

        [Test]
        public void ItCanInsertEntity()
        {
            Assert.IsFalse(players.Any(d => d["Name"] == "George"));
            
            manager.Insert(new JsonObject()
                .Add("$type", "PlayerEntity")
                .Add("Name", "George")
                .Add("_key")
                .Add("_id")
                .Add("_rev")
            );

            Assert.IsTrue(players.Any(d => d["Name"] == "George"));
        }

        [Test]
        public void InsertUpdatesTimestamps()
        {
            var insertedEntity = manager.Insert(new JsonObject()
                .Add("$type", "PlayerEntity")
                .Add("Name", "George")
            );
            
            Assert.IsTrue(insertedEntity.ContainsKey("CreatedAt"));
            Assert.IsTrue(insertedEntity.ContainsKey("UpdatedAt"));

            Assert.AreNotEqual(default(DateTime), insertedEntity["CreatedAt"]);
            Assert.AreNotEqual(default(DateTime), insertedEntity["UpdatedAt"]);
        }

        [Test]
        public void InsertedEntityHasToHaveType()
        {
            Assert.Throws<ArgumentException>(() => {
                manager.Insert(new JsonObject()
                    .Add("Name", "George")
                );
            });
        }

        [Test]
        public void InsertedEntityHasToNotHaveKey()
        {
            Assert.Throws<ArgumentException>(() => {
                manager.Insert(new JsonObject()
                    .Add("$type", "PlayerEntity")
                    .Add("_key", "george")
                    .Add("Name", "George")
                );
            });
        }

        [Test]
        public void InsertingIntoNonExistingCollectionCreatesOne()
        {
            Assert.IsFalse(arango.Collections.ContainsKey("entities_NewEntity"));
            
            manager.Insert(new JsonObject()
                .Add("$type", "NewEntity")
                .Add("Name", "George")
            );
            
            Assert.IsTrue(arango.Collections.ContainsKey("entities_NewEntity"));

            Assert.IsTrue(
                arango
                    .Collections["entities_NewEntity"]
                    .Any(d => d["Name"] == "George")
            );
        }
    }
}