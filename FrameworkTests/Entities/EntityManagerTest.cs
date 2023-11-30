using System;
using System.Linq;
using LightJson;
using NUnit.Framework;
using Unisave.Arango;
using Unisave.Arango.Emulation;
using Unisave.Entities;
using Unisave.Logging;
using Unisave.Serialization;

namespace FrameworkTests.Entities
{
    [TestFixture]
    public class EntityManagerTest
    {
        private ArangoInMemory arango;
        private InMemoryLog log;
        private EntityManager manager;

        private Collection players;
        private Collection motorbikes;

        // uses the default "e_PlayerEntity" name
        private class PlayerEntity : Entity
        {
            public string Name { get; set; }
        }
        
        // uses a custom "motorbikes" name
        [EntityCollectionName("motorbikes")]
        private class MotorbikeEntity : Entity
        {
            public string Name { get; set; }
            public string Model { get; set; }
        }
        
        [SetUp]
        public void SetUp()
        {
            arango = new ArangoInMemory();
            log = new InMemoryLog();
            manager = new EntityManager(arango, log);
            
            players = arango.CreateCollection(
                "e_PlayerEntity",
                CollectionType.Document
            );
            
            motorbikes = arango.CreateCollection(
                "motorbikes",
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
            var john = manager.Find<PlayerEntity>("e_PlayerEntity/john");
            
            Assert.AreEqual("john", john.EntityKey);
            Assert.AreEqual("John Doe", john.Name);
        }
        
        [Test]
        public void ItFindsJohnTheMotorbike()
        {
            var john = manager.Find<MotorbikeEntity>("motorbikes/john");
            
            Assert.AreEqual("john", john.EntityKey);
            Assert.AreEqual("John-deer", john.Name);
        }

        [Test]
        public void ItFindsNull()
        {
            var peterTheMotorbike = manager
                .Find<MotorbikeEntity>("motorbikes/peter");
            
            Assert.IsNull(peterTheMotorbike);
        }

        [Test]
        public void ItFindsNullInNonExistingCollection()
        {
            arango.Collections.Remove("e_PlayerEntity");
            
            var entity = manager.Find<PlayerEntity>("e_PlayerEntity/someKey");
            Assert.IsNull(entity);
        }

        [Test]
        public void ItCanInsertEntity()
        {
            Assert.IsFalse(players.Any(d => d["Name"] == "George"));

            manager.Insert(new PlayerEntity {
                Name = "George"
            });

            Assert.IsTrue(players.Any(d => d["Name"] == "George"));
        }

        [Test]
        public void InsertUpdatesTimestamps()
        {
            var insertedEntity = new PlayerEntity {
                Name = "George"
            };
            
            manager.Insert(insertedEntity);

            Assert.AreNotEqual(
                default(DateTime),
                insertedEntity.CreatedAt
            );
            Assert.AreNotEqual(
                default(DateTime),
                insertedEntity.UpdatedAt
            );
            
            Assert.IsTrue(Math.Abs(
                (
                    DateTime.UtcNow -
                    insertedEntity.CreatedAt
                ).Seconds
            ) < 1);
            Assert.IsTrue(Math.Abs(
                (
                  DateTime.UtcNow -
                  insertedEntity.UpdatedAt
                ).Seconds
            ) < 1);
        }

        [Test]
        public void InsertedEntityHasToNotHaveKey()
        {
            Assert.Throws<ArgumentException>(() => {
                manager.Insert(new PlayerEntity {
                    Name = "George",
                    EntityKey = "george"
                });
            });
        }

        [Test]
        public void InsertingIntoNonExistingCollectionCreatesOne()
        {
            arango.Collections.Remove(
                EntityUtils.CollectionPrefix + "PlayerEntity"
            );
            
            Assert.IsFalse(arango.Collections.ContainsKey(
                EntityUtils.CollectionPrefix + "PlayerEntity")
            );
            
            manager.Insert(new PlayerEntity {
                Name = "George"
            });
            
            Assert.IsTrue(arango.Collections.ContainsKey(
                EntityUtils.CollectionPrefix + "PlayerEntity")
            );

            Assert.IsTrue(
                arango
                    .Collections[EntityUtils.CollectionPrefix + "PlayerEntity"]
                    .Any(d => d["Name"] == "George")
            );
        }

        [Test]
        public void ItCanUpdateEntity()
        {
            Assert.AreEqual(
                "John Doe",
                manager.Find<PlayerEntity>("e_PlayerEntity/john").Name
            );
            
            manager.Update(new PlayerEntity {
                Name = "Johnny",
                EntityKey = "john",
                EntityRevision = "123456789"
                // rev does not match, but we're not careful
            });

            Assert.AreEqual(
                "Johnny",
                manager.Find<PlayerEntity>("e_PlayerEntity/john").Name
            );
        }

        [Test]
        public void UpdatingInNonExistingCollectionThrows()
        {
            arango.Collections.Remove(
                EntityUtils.CollectionFromType(typeof(PlayerEntity))
            );
            
            Assert.Throws<EntityPersistenceException>(() => {
                manager.Update(new PlayerEntity {
                    Name = "Johnny",
                    EntityKey = "john",
                });
            });
        }

        [Test]
        public void UpdatingNonExistingEntityThrows()
        {
            Assert.Throws<EntityPersistenceException>(() => {
                manager.Update(new PlayerEntity {
                    Name = "Jimmy",
                    EntityKey = "jim",
                });
            });
        }

        [Test]
        public void UpdatingUpdatesTimestamp()
        {
            Entity john = manager.Find<PlayerEntity>("e_PlayerEntity/john");
            DateTime createdAt = john.CreatedAt;
            DateTime updatedAt = john.UpdatedAt;

            manager.Update(new PlayerEntity {
                Name = "Johnny",
                EntityKey = "john",
            });

            john = manager.Find<PlayerEntity>("e_PlayerEntity/john");
            
            Assert.AreEqual(createdAt, john.CreatedAt);
            Assert.AreNotEqual(updatedAt, john.UpdatedAt);

            Assert.IsTrue(
                Math.Abs((DateTime.UtcNow - john.UpdatedAt).Seconds) < 1
            );
        }

        [Test]
        public void UpdatedEntityHasToHaveKey()
        {
            Assert.Throws<ArgumentException>(() => {
                manager.Update(new PlayerEntity {
                    Name = "Johnny"
                });
            });
        }

        [Test]
        public void ItCanUpdateCarefully()
        {
            var john = manager.Find<PlayerEntity>("e_PlayerEntity/john");
            
            Assert.AreEqual("John Doe", john.Name);
            
            manager.Update(new PlayerEntity {
                Name = "Johnny",
                EntityKey = "john",
                EntityRevision = john.EntityRevision
            }, carefully: true);
            
            Assert.AreEqual(
                "Johnny",
                manager.Find<PlayerEntity>("e_PlayerEntity/john").Name
            );
        }

        [Test]
        public void UpdatingCarefullyChecksRevisions()
        {
            Assert.AreEqual(
                "John Doe",
                manager.Find<PlayerEntity>("e_PlayerEntity/john").Name
            );
            
            Assert.Throws<EntityRevConflictException>(() => {
                manager.Update(new PlayerEntity {
                    Name = "Johnny",
                    EntityKey = "john",
                    EntityRevision = "123456789"
                }, carefully: true);
            });
            
            Assert.AreEqual(
                "John Doe",
                manager.Find<PlayerEntity>("e_PlayerEntity/john").Name
            );
        }

        [Test]
        public void ItCanDelete()
        {
            Assert.IsNotNull(
                manager.Find<PlayerEntity>("e_PlayerEntity/john")
            );
            
            bool ret = manager.Delete(new PlayerEntity {
                EntityKey = "john",
                Name = "John Doe", // should be ignored
                EntityRevision = "123456789" // should be ignored
            });
            
            Assert.IsTrue(ret);
            
            Assert.IsNull(
                manager.Find<PlayerEntity>("e_PlayerEntity/john")
            );
        }

        [Test]
        public void DeletingNonExistingCollectionReturnsFalse()
        {
            arango.Collections.Remove(
                EntityUtils.CollectionFromType(typeof(PlayerEntity))
            );
            
            bool ret = manager.Delete(new PlayerEntity {
                EntityKey = "john"
            });
            
            Assert.IsFalse(ret);
        }

        [Test]
        public void DeletingNonExistingEntityReturnsFalse()
        {
            bool ret = manager.Delete(new PlayerEntity {
                EntityKey = "jim"
            });
            
            Assert.IsFalse(ret);
        }

        [Test]
        public void DeletedEntityHasToHaveKey()
        {
            Assert.Throws<ArgumentException>(() => {
                manager.Delete(new PlayerEntity {
                    EntityKey = null
                });
            });
        }

        [Test]
        public void ItCanDeleteCarefully()
        {
            var john = manager.Find<PlayerEntity>("e_PlayerEntity/john");
            Assert.IsNotNull(john);
            
            bool ret = manager.Delete(new PlayerEntity {
                EntityKey = "john",
                EntityRevision = john.EntityRevision
            }, carefully: true);
            
            Assert.IsTrue(ret);
            
            Assert.IsNull(manager.Find<PlayerEntity>("e_PlayerEntity/john"));
        }

        [Test]
        public void DeletingCarefullyChecksRevisions()
        {
            Assert.Throws<EntityRevConflictException>(() => {
                manager.Delete(new PlayerEntity {
                    EntityKey = "john",
                    EntityRevision = "123456789"
                }, carefully: true);
            });
        }

        [Test]
        public void ItStoresTimestampsProperly()
        {
            // around the time of entity insertion
            DateTime now = Serializer.FromJson<DateTime>(
                "2020-02-16T14:18:00.000Z"
            );

            Entity john = manager.Find<PlayerEntity>("e_PlayerEntity/john");
            
            Assert.IsTrue(Math.Abs((now - john.CreatedAt).Seconds) < 1);
        }
    }
}