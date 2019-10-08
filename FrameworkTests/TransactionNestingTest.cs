using System;
using Moq;
using NUnit.Framework;
using Unisave;
using Unisave.Database;
using Unisave.Exceptions;
using Unisave.Services;

namespace FrameworkTests
{
    [TestFixture]
    public class TransactionNestingTest
    {
        private Mock<IDatabase> dbMock;

        // transaction level
        private int level;
        
        [SetUp]
        public void SetUp()
        {
            level = 0;
            dbMock = new Mock<IDatabase>();

            dbMock.Setup(db => db.StartTransaction()).Callback(() => level++);
            dbMock.Setup(db => db.CommitTransaction()).Callback(() => level--);
            dbMock.Setup(db => db.RollbackTransaction()).Callback(() => level--);
            dbMock.Setup(db => db.TransactionLevel()).Returns(() => level);
            
            ServiceContainer.Default = new ServiceContainer();
            ServiceContainer.Default.Register<IDatabase>(dbMock.Object);
        }
        
        [TestCase]
        public void ExceptionRollsBackTransaction()
        {
            // also the exception propagates outwards
            Assert.Catch<InvalidOperationException>(() => {
                
                DB.Transaction(() => {
                    throw new InvalidOperationException();
                });
                
            });
            
            dbMock.Verify(db => db.RollbackTransaction(), Times.Once);
        }

        [TestCase]
        public void DeadlockDoesNotRollbackAndPropagatesOut()
        {
            Assert.Catch<DatabaseDeadlockException>(() => {
                
                DB.Transaction(() => {
                    ThrowDeadlock();
                });
                
            });
            
            dbMock.Verify(db => db.RollbackTransaction(), Times.Never);
        }
        
        [TestCase]
        public void DeadlockGetsRetried()
        {
            int attempt = 0;
            DB.Transaction(() => {
                attempt++;
                
                if (attempt == 1)
                    ThrowDeadlock();
            }, 2);
            
            dbMock.Verify(db => db.StartTransaction(), Times.Exactly(2));
            dbMock.Verify(db => db.RollbackTransaction(), Times.Never);
            dbMock.Verify(db => db.CommitTransaction(), Times.Once);
        }
        
        [TestCase]
        public void NestedDeadlockKillsEverything()
        {
            Assert.Throws<DatabaseDeadlockException>(() => {

                int attempt = 0;
                DB.Transaction(() => {
                    DB.Transaction(() => {
                        attempt++;
                        if (attempt == 1)
                            ThrowDeadlock();
                    }, 5);
                    Assert.Fail("Deadlock didn't kill all nested transactions.");
                });

            });
            
            dbMock.Verify(db => db.StartTransaction(), Times.Exactly(2));
            dbMock.Verify(db => db.RollbackTransaction(), Times.Never);
            dbMock.Verify(db => db.CommitTransaction(), Times.Never);
        }
        
        [TestCase]
        public void NestedDeadlockCausesTheTopLevelTransactionToRetry()
        {
            int attempt = 0;
            DB.Transaction(() => {
                attempt++;
                DB.Transaction(() => {
                    if (attempt == 1)
                        ThrowDeadlock();
                    Assert.IsTrue(attempt <= 2);
                }, 5);
            }, 2);
            
            dbMock.Verify(db => db.StartTransaction(), Times.Exactly(4));
            dbMock.Verify(db => db.RollbackTransaction(), Times.Never);
            dbMock.Verify(db => db.CommitTransaction(), Times.Exactly(2));
        }

        private void ThrowDeadlock()
        {
            level = 0;
            throw new DatabaseDeadlockException();
        }
    }
}