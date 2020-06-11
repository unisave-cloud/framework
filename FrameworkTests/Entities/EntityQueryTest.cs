using System;
using FrameworkTests.TestingUtils;
using NUnit.Framework;
using Unisave.Entities;
using Unisave.Facades;

namespace FrameworkTests.Entities
{
    [TestFixture]
    public class EntityQueryTest : BackendTestCase
    {
        private class PlayerEntity : Entity
        {
            public string name;
            public DateTime premiumUntil = DateTime.UtcNow;
        }
        
        [Test]
        public void ItCanFilterByDateTime()
        {
            new PlayerEntity {
                name = "HasPremium",
                premiumUntil = DateTime.UtcNow.AddHours(1)
            }.Save();
            
            new PlayerEntity {
                name = "NoPremium",
                premiumUntil = DateTime.UtcNow.AddHours(-1)
            }.Save();

            var results = DB.TakeAll<PlayerEntity>()
                .Filter(entity => entity.premiumUntil > DateTime.UtcNow)
                .Get();
            
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual("HasPremium", results[0].name);
        }
    }
}