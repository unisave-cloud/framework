using System;
using System.Linq;
using LightJson;
using NUnit.Framework;
using Unisave.Arango;
using Unisave.Arango.Emulation;
using Unisave.Entities;
using Unisave.Serialization;

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
                EntityUtils.CollectionPrefix + "PlayerEntity",
                CollectionType.Document
            );
            
            motorbikes = arango.CreateCollection(
                EntityUtils.CollectionPrefix + "MotorbikeEntity",
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
            Assert.AreEqual("PlayerEntity", john["$type"].AsString);
            
            Assert.AreEqual(null, john["Model"].AsString);
        }
        
        [Test]
        public void ItFindsJohnTheMotorbike()
        {
            var john = manager.Find("MotorbikeEntity", "john");
            
            Assert.AreEqual("john", john["_key"].AsString);
            Assert.AreEqual("John-deer", john["Name"].AsString);
            Assert.AreEqual("MotorbikeEntity", john["$type"].AsString);
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

            Assert.AreNotEqual(
                default(DateTime),
                Serializer.FromJson<DateTime>(insertedEntity["CreatedAt"])
            );
            Assert.AreNotEqual(
                default(DateTime),
                Serializer.FromJson<DateTime>(insertedEntity["UpdatedAt"])
            );
            
            Assert.IsTrue(Math.Abs(
                (
                    DateTime.UtcNow -
                    Serializer.FromJson<DateTime>(insertedEntity["CreatedAt"])
                ).Seconds
            ) < 1);
            Assert.IsTrue(Math.Abs(
                (
                  DateTime.UtcNow -
                  Serializer.FromJson<DateTime>(insertedEntity["UpdatedAt"])
                ).Seconds
            ) < 1);
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
            Assert.IsFalse(arango.Collections.ContainsKey(
                EntityUtils.CollectionPrefix + "NewEntity")
            );
            
            manager.Insert(new JsonObject()
                .Add("$type", "NewEntity")
                .Add("Name", "George")
            );
            
            Assert.IsTrue(arango.Collections.ContainsKey(
                EntityUtils.CollectionPrefix + "NewEntity")
            );

            Assert.IsTrue(
                arango
                    .Collections[EntityUtils.CollectionPrefix + "NewEntity"]
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
                .Add("_id", EntityUtils.CollectionPrefix + "PlayerEntity/john")
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
            
            Assert.AreEqual(
                createdAt,
                Serializer.FromJson<DateTime>(john["CreatedAt"])
            );
            Assert.AreNotEqual(
                updatedAt,
                Serializer.FromJson<DateTime>(john["UpdatedAt"])
            );

            Assert.IsTrue(Math.Abs(
                (
                    DateTime.UtcNow -
                    Serializer.FromJson<DateTime>(john["UpdatedAt"])
                 ).Seconds
            ) < 1);
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

        [Test]
        public void ItCanDelete()
        {
            Assert.IsNotNull(manager.Find("PlayerEntity", "john"));
            
            manager.Delete(new JsonObject()
                .Add("$type", "PlayerEntity")
                .Add("_key", "john")
                .Add("_rev", "123456789") // should be ignored
                .Add("Foo", "bar")
                .Add("Name", "John Doe")
            );
            
            Assert.IsNull(manager.Find("PlayerEntity", "john"));
        }

        [Test]
        public void DeletingNonExistingCollectionThrows()
        {
            Assert.Throws<EntityPersistenceException>(() => {
                manager.Delete(new JsonObject()
                    .Add("$type", "NonExistingEntity")
                    .Add("_key", "john")
                    .Add("Foo", "bar")
                    .Add("Name", "John Doe")
                );
            });
        }

        [Test]
        public void DeletingNonExistingEntityThrows()
        {
            Assert.Throws<EntityPersistenceException>(() => {
                manager.Delete(new JsonObject()
                    .Add("$type", "PlayerEntity")
                    .Add("_key", "jim")
                    .Add("Name", "Jimmy")
                );
            });
        }

        [Test]
        public void DeletedEntityHasToHaveType()
        {
            Assert.Throws<ArgumentException>(() => {
                manager.Delete(new JsonObject()
                    .Add("_key", "john")
                );
            });
        }

        [Test]
        public void DeletedEntityHasToHaveKey()
        {
            Assert.Throws<ArgumentException>(() => {
                manager.Delete(new JsonObject()
                    .Add("$type", "PlayerEntity")
                );
            });
        }

        [Test]
        public void ItCanDeleteCarefully()
        {
            Assert.IsNotNull(manager.Find("PlayerEntity", "john"));
            string rev = manager.Find("PlayerEntity", "john")["_rev"];
            
            manager.Delete(
                new JsonObject()
                    .Add("$type", "PlayerEntity")
                    .Add("_key", "john")
                    .Add("_rev", rev)
                    .Add("Name", "John Doe"),
                carefully: true
            );
            
            Assert.IsNull(manager.Find("PlayerEntity", "john"));
        }

        [Test]
        public void DeletingCarefullyChecksRevisions()
        {
            Assert.Throws<EntityRevConflictException>(() => {
                manager.Delete(
                    new JsonObject()
                        .Add("$type", "PlayerEntity")
                        .Add("_key", "john")
                        .Add("_rev", "123456789")
                        .Add("Name", "John Doe"),
                    carefully: true
                );
            });
        }

        [Test]
        public void ItStoresTimestampsProperly()
        {
            // around the time of entity insertion
            DateTime now = Serializer.FromJson<DateTime>(
                "2020-02-16T14:18:00.000Z"
            );

            JsonObject john = manager.Find("PlayerEntity", "john");
            DateTime created = Serializer.FromJson<DateTime>(john["CreatedAt"]);
            
            Assert.IsTrue(Math.Abs((now - created).Seconds) < 1);
        }
    }
}