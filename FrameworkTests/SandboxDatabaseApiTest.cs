using System;
using System.Linq;
using LightJson;
using Moq;
using NUnit.Framework;
using Unisave.Database;
using Unisave.Database.Query;
using Unisave.Runtime;
using Unisave.Serialization;
using Unisave.Services;

namespace FrameworkTests
{
    [TestFixture]
    public class SandboxDatabaseApiTest
    {
        private IDatabase database;
        private Mock<ApiChannel> channelMock;
        
        [SetUp]
        public void SetUp()
        {
            channelMock = new Mock<ApiChannel>("db");
            
            database = new SandboxDatabaseApi(
                channelMock.Object
            );
        }
        
        [Test]
        public void ItCanCreateEntity()
        {
            var entity = new RawEntity();
            var now = DateTime.UtcNow;
            now = new DateTime(
                now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second
            );

            channelMock.Setup(channel =>
                channel.SendJsonMessage(
                    100,
                    It.Is<JsonObject>(j =>
                        j["entity"].ToString() == entity.ToJson().ToString()
                    )
                )
            ).Returns(
                new JsonObject()
                    .Add("entity_id", "new-entity-id")
                    .Add("created_at", Serializer.ToJson(now))
                    .Add("updated_at", Serializer.ToJson(now))
                    .Add("owner_ids",
                        new EntityOwnerIds(new string[] {"some-player"}, false)
                            .ToJson()
                    )
            );
            
            database.SaveEntity(entity);
            
            Assert.AreEqual("new-entity-id", entity.id);
            Assert.AreEqual(now, entity.createdAt);
            Assert.AreEqual(now, entity.updatedAt);
            Assert.AreEqual("some-player", entity.ownerIds.GetKnownOwners().First());
            
            channelMock.VerifyAll();
        }
        
        [Test]
        public void ItCanUpdateEntity()
        {
            var entity = new RawEntity {
                id = "some-entity-id"
            };
            var now = DateTime.UtcNow;
            now = new DateTime(
                now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second
            );

            channelMock.Setup(channel =>
                channel.SendJsonMessage(
                    100,
                    It.Is<JsonObject>(j => j.ContainsKey("entity"))
                )
            ).Returns(
                new JsonObject()
                    .Add("entity_id", entity.id)
                    .Add("created_at", Serializer.ToJson(entity.createdAt))
                    .Add("updated_at", Serializer.ToJson(now))
                    .Add("owner_ids",
                        new EntityOwnerIds(new string[] {"some-player"}, false)
                            .ToJson()
                    )
            );
            
            database.SaveEntity(entity);
            
            Assert.AreEqual("some-entity-id", entity.id);
            Assert.AreNotEqual(now, entity.createdAt);
            Assert.AreEqual(now, entity.updatedAt);
            Assert.AreEqual("some-player", entity.ownerIds.GetKnownOwners().First());
            
            channelMock.VerifyAll();
        }

        [Test]
        public void ItCanLoadEntity()
        {
            var entity = new RawEntity {
                id = "some-entity-id"
            };
            
            channelMock.Setup(channel =>
                channel.SendJsonMessage(
                    101,
                    It.Is<JsonObject>(j => j["entity_id"] == entity.id)
                )
            ).Returns(
                new JsonObject()
                    .Add("entity", entity.ToJson())
            );

            var loadedEntity = database.LoadEntity(entity.id);
            
            Assert.AreEqual(
                entity.ToJson().ToString(),
                loadedEntity.ToJson().ToString()
            );
            
            channelMock.VerifyAll();
        }

        [Test]
        public void ItCanGetEntityOwners()
        {
            const string entityId = "some-entity-id";
            const int enumeratorId = 42;

            int nextResponse = 0;
            JsonObject[] responses = {
                new JsonObject()
                    .Add("done", false)
                    .Add("item", "first"),
                new JsonObject()
                    .Add("done", false)
                    .Add("item", "second"),
                new JsonObject()
                    .Add("done", true),
            };
            
            channelMock.Setup(channel =>
                channel.SendJsonMessage(
                    102,
                    It.Is<JsonObject>(j => j["entity_id"] == entityId)
                )
            ).Returns(
                new JsonObject()
                    .Add("enumerator_id", enumeratorId)
            );
            
            channelMock.Setup(channel =>
                channel.SendJsonMessage(
                    103,
                    It.Is<JsonObject>(j => j["enumerator_id"] == enumeratorId)
                )
            ).Returns(() => responses[nextResponse++]);

            channelMock.Setup(channel =>
                channel.SendJsonMessage(
                    104,
                    It.Is<JsonObject>(j => j["enumerator_id"] == enumeratorId)
                )
            );

            int index = 0;
            foreach (string owner in database.GetEntityOwners(entityId))
            {
                if (index == 0)
                    Assert.AreEqual("first", owner);
                if (index == 1)
                    Assert.AreEqual("second", owner);
                index++;
            }
            Assert.AreEqual(2, index);
            
            channelMock.VerifyAll();
        }

        [Test]
        public void ItTestsEntityOwners()
        {
            const string entityId = "some-entity-id";
            const string playerId = "some-player-id";
            
            channelMock.Setup(channel =>
                channel.SendJsonMessage(
                    105,
                    It.Is<JsonObject>(j =>
                        j["entity_id"] == entityId &&
                        j["player_id"] == playerId
                    )
                )
            ).Returns(
                new JsonObject()
                    .Add("is_owner", true)
            );

            Assert.IsTrue(
                database.IsEntityOwner(entityId, playerId)
            );
            
            channelMock.VerifyAll();
        }

        [Test]
        public void ItDeletesEntities()
        {
            const string entityId = "some-entity-id";
            
            channelMock.Setup(channel =>
                channel.SendJsonMessage(
                    106,
                    It.Is<JsonObject>(j => j["entity_id"] == entityId)
                )
            ).Returns(
                new JsonObject()
                    .Add("was_deleted", true)
            );

            Assert.IsTrue(
                database.DeleteEntity(entityId)
            );
            
            channelMock.VerifyAll();
        }

        [Test]
        public void ItQueriesEntities()
        {
            const int enumeratorId = 42;
            
            EntityQuery query = new EntityQuery();

            RawEntity first = new RawEntity { type = "FirstEntity" };
            RawEntity second = new RawEntity { type = "SecondEntity" };

            int nextResponse = 0;
            JsonObject[] responses = {
                new JsonObject()
                    .Add("done", false)
                    .Add("item", first.ToJson()),
                new JsonObject()
                    .Add("done", false)
                    .Add("item", second.ToJson()),
                new JsonObject()
                    .Add("done", true),
            };
            
            channelMock.Setup(channel =>
                channel.SendJsonMessage(
                    107,
                    It.Is<JsonObject>(j =>
                        j["query"].ToString() == query.ToJson().ToString()
                    )
                )
            ).Returns(
                new JsonObject()
                    .Add("enumerator_id", enumeratorId)
            );
            
            channelMock.Setup(channel =>
                channel.SendJsonMessage(
                    108,
                    It.Is<JsonObject>(j => j["enumerator_id"] == enumeratorId)
                )
            ).Returns(() => responses[nextResponse++]);

            channelMock.Setup(channel =>
                channel.SendJsonMessage(
                    109,
                    It.Is<JsonObject>(j => j["enumerator_id"] == enumeratorId)
                )
            );

            int index = 0;
            foreach (RawEntity entity in database.QueryEntities(query))
            {
                if (index == 0)
                    Assert.AreEqual(
                        first.ToJson().ToString(),
                        entity.ToJson().ToString()
                    );
                if (index == 1)
                    Assert.AreEqual(
                        second.ToJson().ToString(),
                        entity.ToJson().ToString()
                    );
                index++;
            }
            Assert.AreEqual(2, index);
            
            channelMock.VerifyAll();
        }
    }
}