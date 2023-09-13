using System;
using Moq;
using NUnit.Framework;
using Unisave;
using Unisave.Contracts;
using Unisave.Entities;
using Unisave.Facades;
using Unisave.Foundation;
using Unisave.Serialization;
using Unisave.Serialization.Context;

namespace FrameworkTests.Entities
{
    [TestFixture]
    public class MassAssignmentTest
    {
        private class MyEntity : Entity
        {
            [Fillable]
            public string foo;

            [Fillable]
            public string Bar { get; set; }

            public string secret;
        }

        [SetUp]
        public void SetUp()
        {
            var app = new Application(new Type[] {});
            var arango = new Mock<IArango>();
            var log = new Mock<ILog>();
            var manager = new Mock<EntityManager>(arango.Object, log.Object);
            app.Services.RegisterInstance<EntityManager>(manager.Object);
            Facade.SetApplication(app);
        }
        
        [Test]
        public void ClientModifiedEntityCantBeSavedOnServer()
        {
            var clientEntity = new MyEntity {
                EntityKey = "some-key", // modified, not created
                foo = "foo",
                Bar = "Bar"
            };

            var json = Serializer.ToJson(
                clientEntity,
                SerializationContext.ClientToServer
            );

            var receivedEntity = Serializer.FromJson<MyEntity>(
                json,
                DeserializationContext.ClientToServer
            );
            
            Assert.NotNull(receivedEntity.EntityId);
            
            Assert.Throws<EntitySecurityException>(() => {
                receivedEntity.Save();
            });
            
            Assert.Throws<EntitySecurityException>(() => {
                receivedEntity.Refresh();
            });
            
            Assert.Throws<EntitySecurityException>(() => {
                receivedEntity.Delete();
            });
        }
        
        [Test]
        public void ClientCreatedEntityCanBeSavedOnServer()
        {
            var clientEntity = new MyEntity {
                EntityKey = null, // created by the client
                foo = "foo",
                Bar = "Bar"
            };

            var json = Serializer.ToJson(
                clientEntity,
                SerializationContext.ClientToServer
            );

            var receivedEntity = Serializer.FromJson<MyEntity>(
                json,
                DeserializationContext.ClientToServer
            );
            
            Assert.IsNull(receivedEntity.EntityId);
            
            Assert.DoesNotThrow(() => {
                receivedEntity.Save();
            });
            
            // refresh and delete make no sense as the ID is null
        }

        [Test]
        public void FieldsCanBeFilled()
        {
            var dataEntity = new MyEntity {
                EntityKey = "data-entity-key",
                EntityRevision = "data-entity-revision",
                foo = "foo",
                Bar = "Bar",
                secret = "mocked-with-secret"
            };
            
            var serverEntity = new MyEntity {
                EntityKey = "server-entity-key",
                EntityRevision = "server-entity-revision",
                foo = "x",
                Bar = "x",
                secret = "true-secret"
            };
            
            serverEntity.FillWith(dataEntity);
            
            // data filled
            Assert.AreEqual("foo", serverEntity.foo);
            Assert.AreEqual("Bar", serverEntity.Bar);
            
            // data preserved
            Assert.AreEqual("true-secret", serverEntity.secret);
            Assert.AreEqual("server-entity-key", serverEntity.EntityKey);
            Assert.AreEqual("server-entity-revision", serverEntity.EntityRevision);
        }
    }
}