using FrameworkTests.Stubs;
using Moq;
using NUnit.Framework;
using Unisave;
using Unisave.Contracts;
using Unisave.Database;
using Unisave.Services;

namespace FrameworkTests
{
    [TestFixture]
    public class EntityTest
    {
        private IDatabase db;
        private Mock<IDatabase> dbMock;
        
        [SetUp]
        public void SetUp()
        {
            dbMock = new Mock<IDatabase>();
            db = dbMock.Object;
            ServiceContainer.Default = new ServiceContainer();
            ServiceContainer.Default.Register<IDatabase>(db);
        }
        
        [Test]
        public void SavingEntityPropagatesToDatabase()
        {
            dbMock.Setup(mock => mock.SaveEntity(It.IsAny<RawEntity>()));
            
            var e = new SomeEntity();
            e.Save();

            dbMock.VerifyAll();
        }

        [Test]
        public void TestingOwnershipForKnownPlayerIsResolvedImmediately()
        {
            var e = new SomeEntity();
            e.Owners.Add(new UnisavePlayer("real-owner"));
            e.Owners.OwnerIds.IsComplete = false;

            Assert.IsTrue(e.Owners.Contains(new UnisavePlayer("real-owner")));
        }

        [Test]
        public void TestingOwnershipForUnknownPlayerQueriesDatabase()
        {
            var e = new SomeEntity();
            e.Owners.Add(new UnisavePlayer("real-owner"));
            e.Owners.OwnerIds.IsComplete = false;

            dbMock
                .Setup(mock => mock.IsEntityOwner(e.EntityId, "maybe-owner"))
                .Returns(true);

            Assert.IsTrue(e.Owners.Contains(new UnisavePlayer("maybe-owner")));
            
            dbMock.VerifyAll();
        }
    }
}