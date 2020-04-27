using System;
using LightJson;
using NUnit.Framework;
using Unisave;
using Unisave.Entities;

namespace FrameworkTests.Entities
{
    [TestFixture]
    public class EntityAttributesTest
    {
        public class PlayerEntity : Entity
        {
            public string Name { get; set; }
            public int Coins { get; set; }

            public bool HasName => !string.IsNullOrEmpty(Name);
            public int foo = 42;
        }

        [Test]
        public void AttributesAreProperlyGathered()
        {
            var player = new PlayerEntity();
            player.Name = "John";
            player.Coins = 100;

            var attr = player.GetAttributes();
            
            Assert.AreEqual("PlayerEntity", attr["$type"].AsString);
            Assert.IsTrue(attr["_key"].IsNull);
            Assert.IsTrue(attr["_rev"].IsNull);
            Assert.IsTrue(attr["_id"].IsNull);
            Assert.AreEqual("John", attr["Name"].AsString);
            Assert.AreEqual(100, attr["Coins"].AsInteger);
            Assert.AreEqual(default(DateTime), attr["CreatedAt"].AsDateTime);
            Assert.AreEqual(default(DateTime), attr["UpdatedAt"].AsDateTime);
            Assert.AreEqual(8, attr.Count);
        }

        [Test]
        public void AttributesAreProperlyDistributed()
        {
            var player = new PlayerEntity();
            
            player.SetAttributes(new JsonObject()
                .Add("Name", "Peter")
                .Add("Coins", 50)
                .Add("_id")
                .Add("_key")
                .Add("_rev")
                .Add("$type", "PlayerEntity")
            );
            
            Assert.AreEqual("Peter", player.Name);
            Assert.AreEqual(50, player.Coins);
            Assert.IsNull(player.EntityId);
            Assert.IsNull(player.EntityKey);
            Assert.IsNull(player.EntityRevision);

            player["_id"] = "entities_PlayerEntity/peter";
            Assert.AreEqual("entities_PlayerEntity/peter", player.EntityId);
            Assert.AreEqual("peter", player.EntityKey);
            Assert.IsNull(player.EntityRevision);

            player["_rev"] = "123456789";
            Assert.AreEqual("123456789", player.EntityRevision);
        }

        [Test]
        public void SettingNonExistingAttributeIsIgnored()
        {
            Assert.DoesNotThrow(() => {
                var player = new PlayerEntity();
                player["some-nonsense-here"] = 42;
            });
        }
    }
}