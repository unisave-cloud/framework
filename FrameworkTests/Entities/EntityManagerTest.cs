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
                'Name': 'John Doe',
                'CreatedAt': '2020-02-16T14:18:00',
                'UpdatedAt': '2020-02-16T14:18:00'
            }");
            
            players.Add(@"{
                '_key': 'peter',
                'Name': 'Peter',
                'CreatedAt': '2020-02-16T14:18:00',
                'UpdatedAt': '2020-02-16T14:18:00'
            }");
            
            motorbikes.Add(@"{
                '_key': 'john',
                'Name': 'John-deer',
                'Model': 'latest',
                'CreatedAt': '2020-02-16T14:18:00',
                'UpdatedAt': '2020-02-16T14:18:00'
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

        [Test]
        public void ItCanUpdateEntity()
        {
            Assert.AreEqual(
                "John Doe",
                manager.Find("PlayerEntity", "john")["Name"].AsString
            );
            
            manager.Update(new JsonObject()
                .Add("$type", "PlayerEntity")
                .Add("Name", "Johnny")
                .Add("Foo", "bar")
                .Add("_key", "john")
                .Add("_id", "entities_PlayerEntity/john")
                .Add("_rev", "123456789")
                // rev does not match, but we're not careful
            );

            Assert.AreEqual(
                "Johnny",
                manager.Find("PlayerEntity", "john")["Name"].AsString
            );
            
            Assert.AreEqual(
                "bar",
                manager.Find("PlayerEntity", "john")["Foo"].AsString
            );
        }

        [Test]
        public void UpdatingInNonExistingCollectionThrows()
        {
            Assert.Throws<EntityPersistenceException>(() => {
                manager.Update(new JsonObject()
                    .Add("$type", "NonExistingEntity")
                    .Add("_key", "john")
                    .Add("Name", "Johnny")
                );
            });
        }

        [Test]
        public void UpdatingNonExistingEntityThrows()
        {
            Assert.Throws<EntityPersistenceException>(() => {
                manager.Update(new JsonObject()
                    .Add("$type", "PlayerEntity")
                    .Add("_key", "jim")
                    .Add("Name", "Jimmy")
                );
            });
        }

        [Test]
        public void UpdatingUpdatesTimestamp()
        {
            JsonObject john = manager.Find("PlayerEntity", "john");
            DateTime createdAt = john["CreatedAt"];
            DateTime updatedAt = john["UpdatedAt"];

            manager.Update(new JsonObject()
                .Add("$type", "PlayerEntity")
                .Add("_key", "john")
                .Add("Name", "Johnny")
            );
            
            john = manager.Find("PlayerEntity", "john");
            
            Assert.AreEqual(createdAt, john["CreatedAt"].AsDateTime);
            Assert.AreNotEqual(updatedAt, john["UpdatedAt"].AsDateTime);
        }

        [Test]
        public void UpdatedEntityHasToHaveKey()
        {
            Assert.Throws<ArgumentException>(() => {
                manager.Update(new JsonObject()
                    .Add("$type", "PlayerEntity")
                    .Add("Name", "Johnny")
                );
            });
        }
        
        [Test]
        public void UpdatedEntityHasToHaveType()
        {
            Assert.Throws<ArgumentException>(() => {
                manager.Update(new JsonObject()
                    .Add("_key", "john")
                    .Add("Name", "Johnny")
                );
            });
        }

        [Test]
        public void ItCanUpdateCarefully()
        {
            Assert.AreEqual(
                "John Doe",
                manager.Find("PlayerEntity", "john")["Name"].AsString
            );
            
            var rev = manager.Find("PlayerEntity", "john")["_rev"].AsString;
            
            manager.Update(
                new JsonObject()
                    .Add("$type", "PlayerEntity")
                    .Add("_key", "john")
                    .Add("_rev", rev)
                    .Add("Name", "Johnny"),
                carefully: true
            );
            
            Assert.AreEqual(
                "Johnny",
                manager.Find("PlayerEntity", "john")["Name"].AsString
            );
        }

        [Test]
        public void UpdatingCarefullyChecksRevisions()
        {
            Assert.AreEqual(
                "John Doe",
                manager.Find("PlayerEntity", "john")["Name"].AsString
            );
            
            Assert.Throws<EntityRevConflictException>(() => {
                manager.Update(
                    new JsonObject()
                        .Add("$type", "PlayerEntity")
                        .Add("_key", "john")
                        .Add("_rev", "123456789")
                        .Add("Name", "Johnny"),
                    carefully: true
                );
            });
            
            Assert.AreEqual(
                "John Doe",
                manager.Find("PlayerEntity", "john")["Name"].AsString
            );
        }
    }
}