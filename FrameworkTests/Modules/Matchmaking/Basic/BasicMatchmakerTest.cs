using System;
using System.Linq;
using FrameworkTests.TestingUtils;
using NUnit.Framework;
using Unisave;
using Unisave.Arango.Expressions;
using Unisave.Facades;
using Unisave.Modules.Matchmaking;
using Unisave.Modules.Matchmaking.Exceptions;
using Unisave.Serialization;

namespace FrameworkTests.Modules.Matchmaking.Basic
{
    [TestFixture]
    public class BasicMatchmakerTest : BackendTestCase
    {
        private BmPlayerEntity john, peter;
        
        [SetUp]
        public void SetUp()
        {
            // populate database
            john = new BmPlayerEntity { Name = "John" };
            peter = new BmPlayerEntity { Name = "Pater" };
            john.Save();
            peter.Save();

            // match pairs by default
            BmMatchmakerFacet.matching = "pairs";
        }

        private BasicMatchmakerEntity CreateEntity()
        {
            var entity = new BasicMatchmakerEntity {
                MatchmakerName = typeof(BmMatchmakerFacet).Name
            };
            entity.Save();
            return entity;
        }

        [Test]
        public void PlayerHasToBeAuthorizedToInsertTicket()
        {
            throw new NotImplementedException();
            //Assert.Throws<AuthenticationException>()
        }

        [Test]
        public void MatchmakerHasToSetEntityRelations()
        {
            // see the TODOs below and inside the matchmaker facet
            // (this test method is just not to forget, delete it)
            throw new NotImplementedException();
        }

        [Test]
        public void TicketCanBeInserted()
        {
            ActingAs(john);
            var ticket = new BmMatchmakerTicket(john.EntityId);
            OnFacet<BmMatchmakerFacet>().CallSync("JoinMatchmaker", ticket);

            // ticket will be modified by preprocessing,
            // so make sure it matches in the assertion below
            ticket.someValue = 42;

            var entity = DB.First<BasicMatchmakerEntity>();
            UAssert.AreJsonEqual(ticket, entity.Tickets[0]);
        }
        
        [Test]
        public void NullTicketOwnerGetsSetToTheCaller()
        {
            ActingAs(john);
            var ticket = new BmMatchmakerTicket();
            OnFacet<BmMatchmakerFacet>().CallSync("JoinMatchmaker", ticket);

            var entity = DB.First<BasicMatchmakerEntity>();
            Assert.AreEqual(
                john.EntityId,
                entity.DeserializeTickets<BmMatchmakerTicket>()[0].PlayerId
            );
        }

        [Test]
        public void TicketPreparationGetsCalled()
        {
            ActingAs(john);
            OnFacet<BmMatchmakerFacet>().CallSync(
                "JoinMatchmaker", new BmMatchmakerTicket()
            );

            var entity = DB.First<BasicMatchmakerEntity>();
            Assert.AreEqual(
                42,
                entity.DeserializeTickets<BmMatchmakerTicket>()[0].someValue
            );
        }

        [Test]
        public void InsertingAlreadyInsertedTicketDoesNotDuplicateIt()
        {
            ActingAs(john);
            
            // insert once
            var ticket = new BmMatchmakerTicket(john.EntityId);
            OnFacet<BmMatchmakerFacet>().CallSync("JoinMatchmaker", ticket);
            
            // insert twice
            OnFacet<BmMatchmakerFacet>().CallSync("JoinMatchmaker", ticket);
            
            // check only once
            var entity = DB.First<BasicMatchmakerEntity>();
            Assert.AreEqual(1, entity.Tickets.Count);
        }

        [Test]
        public void InsertingToBeNotifiedTicketCancelsTheNotification()
        {
            var entity = CreateEntity();
            entity.Notifications.Add(new BasicMatchmakerEntity.Notification {
                playerId = john.EntityId,
                matchId = "some-match-id"
            });
            entity.Save();
            
            var ticket = new BmMatchmakerTicket(john.EntityId);
            ActingAs(john);
            OnFacet<BmMatchmakerFacet>().CallSync("JoinMatchmaker", ticket);
            
            entity.Refresh();
            Assert.IsEmpty(entity.Notifications);
        }

        [Test]
        public void DeprecatedEntityGetsRecreated()
        {
            var entity = new BasicMatchmakerEntity {
                Version = -1,
                MatchmakerName = typeof(BmMatchmakerFacet).Name
            };
            entity.Save();

            ActingAs(john);
            var ticket = new BmMatchmakerTicket(john.EntityId);
            OnFacet<BmMatchmakerFacet>().CallSync("JoinMatchmaker", ticket);
            
            var newEntity = DB.First<BasicMatchmakerEntity>();
            Assert.AreNotEqual(entity.EntityId, newEntity.EntityId);
        }

        [Test]
        public void PollingReturnsValueOnlyWhenPlayerShouldBeNotified()
        {
            var entity = CreateEntity();
            // start waiting to prevent exception from being thrown
            ActingAs(john);
            OnFacet<BmMatchmakerFacet>().CallSync(
                "JoinMatchmaker", new BmMatchmakerTicket(john.EntityId)
            );
            
            var match = OnFacet<BmMatchmakerFacet>().CallSync<BmMatchEntity>(
                "PollMatchmaker", false
            );
            Assert.IsNull(match);
            
            match = new BmMatchEntity();
            match.Save();
            entity.Refresh();
            entity.Notifications.Add(new BasicMatchmakerEntity.Notification {
                playerId = john.EntityId,
                matchId = match.EntityId
            });
            entity.Save();
            
            var polledMatch = OnFacet<BmMatchmakerFacet>()
                .CallSync<BmMatchEntity>("PollMatchmaker", false);
            Assert.IsNotNull(polledMatch);
            Assert.AreEqual(match.EntityId, polledMatch.EntityId);
        }

        [Test]
        public void PollingWhenNotWaitingThrows()
        {
            ActingAs(john);
            
            Assert.Catch<UnknownPlayerPollingException>(() => {
                OnFacet<BmMatchmakerFacet>()
                    .CallSync<BmMatchEntity>("PollMatchmaker", false);
            });
        }

        [Test]
        public void PlayerCanLeaveTheMatchmaker()
        {
            ActingAs(john);
            
            var entity = CreateEntity();
            OnFacet<BmMatchmakerFacet>().CallSync(
                "JoinMatchmaker", new BmMatchmakerTicket(john.EntityId)
            );
            
            entity.Refresh();
            Assert.IsNotEmpty(entity.Tickets);
            
            var polledMatch = OnFacet<BmMatchmakerFacet>()
                .CallSync<BmMatchEntity>("PollMatchmaker", true);
            Assert.IsNull(polledMatch);
            
            entity.Refresh();
            Assert.IsEmpty(entity.Tickets);
        }
        
        [Test]
        public void PlayerMightWantToLeaveTheMatchmakerButBeMatched()
        {
            ActingAs(john);
            
            var entity = CreateEntity();
            OnFacet<BmMatchmakerFacet>().CallSync(
                "JoinMatchmaker", new BmMatchmakerTicket(john.EntityId)
            );
            
            entity.Refresh();
            Assert.IsNotEmpty(entity.Tickets);
            
            var match = new BmMatchEntity();
            match.Save();
            entity.Refresh();
            entity.Notifications.Add(new BasicMatchmakerEntity.Notification {
                playerId = john.EntityId,
                matchId = match.EntityId
            });
            entity.Save();
            
            var polledMatch = OnFacet<BmMatchmakerFacet>()
            .CallSync<BmMatchEntity>("PollMatchmaker", true);
            Assert.IsNotNull(polledMatch);
            Assert.AreEqual(match.EntityId, polledMatch.EntityId);
        }

        [Test]
        public void TestPairingProcess()
        {
            var entity = CreateEntity();
            ActingAs(john);
            OnFacet<BmMatchmakerFacet>()
                .CallSync(
                "JoinMatchmaker", new BmMatchmakerTicket(john.EntityId)
            );
            
            Assert.IsNull(OnFacet<BmMatchmakerFacet>()
                .CallSync<BmMatchEntity>("PollMatchmaker", false));
            
            ActingAs(peter);
            OnFacet<BmMatchmakerFacet>().CallSync(
                "JoinMatchmaker", new BmMatchmakerTicket(peter.EntityId)
            );
            
            // does the matching
            ActingAs(john);
            var johnMatch = OnFacet<BmMatchmakerFacet>()
                .CallSync<BmMatchEntity>("PollMatchmaker", false);
            entity.Refresh();
            Assert.IsEmpty(entity.Tickets); // empties tickets
            Assert.IsNotEmpty(entity.Notifications); // fills notifications
            
            // returns the last notification
            ActingAs(peter);
            var peterMatch = OnFacet<BmMatchmakerFacet>()
                .CallSync<BmMatchEntity>("PollMatchmaker", false);
            
            Assert.NotNull(johnMatch);
            Assert.NotNull(peterMatch);
            
            Assert.AreEqual(johnMatch.EntityId, peterMatch.EntityId);
            
            // TODO: entity ownership is now relations
//            Assert.IsTrue(johnMatch.Owners.Contains(john));
//            Assert.IsTrue(johnMatch.Owners.Contains(peter));
            
            entity.Refresh();
            Assert.IsEmpty(entity.Tickets);
            Assert.IsEmpty(entity.Notifications);
        }

        [Test]
        public void TicketShouldNotBeMatchedTwice()
        {
            BmMatchmakerFacet.matching = "match-twice";

            ActingAs(john);
            OnFacet<BmMatchmakerFacet>().CallSync(
                "JoinMatchmaker", new BmMatchmakerTicket(john.EntityId)
            );
            
            Assert.Catch<InvalidOperationException>(() => {
                OnFacet<BmMatchmakerFacet>()
                    .CallSync<BmMatchEntity>("PollMatchmaker", false);
            });
        }
        
        [Test]
        public void CannotStartMatchThatIsAlreadySaved()
        {
            BmMatchmakerFacet.matching = "match-saved";

            ActingAs(john);
            OnFacet<BmMatchmakerFacet>().CallSync(
                "JoinMatchmaker", new BmMatchmakerTicket(john.EntityId)
            );
            
            Assert.Catch<ArgumentException>(() => {
                OnFacet<BmMatchmakerFacet>()
                    .CallSync<BmMatchEntity>("PollMatchmaker", false);
            });
        }

        [Test]
        public void ExpiredTicketsGetCleanedUp()
        {
            var entity = CreateEntity();
            ActingAs(john);
            OnFacet<BmMatchmakerFacet>().CallSync(
                "JoinMatchmaker", new BmMatchmakerTicket(john.EntityId)
            );
            
            // time warp
            entity.Refresh();
            var tickets = entity.DeserializeTickets<BmMatchmakerTicket>();
            tickets[0].LastPollAt = DateTime.UtcNow.AddHours(-1);
            entity.SerializeTickets(tickets);
            entity.Save();

            Assert.Catch<UnknownPlayerPollingException>(() => {
                OnFacet<BmMatchmakerFacet>()
                    .CallSync<BmMatchEntity>("PollMatchmaker", false);
            });
            
            entity.Refresh();
            Assert.IsEmpty(entity.Tickets);
        }
        
        [Test]
        public void PollResetsTicketExpirationTime()
        {
            var entity = CreateEntity();
            ActingAs(john);
            OnFacet<BmMatchmakerFacet>().CallSync(
                "JoinMatchmaker", new BmMatchmakerTicket(john.EntityId)
            );
            
            // time warp
            entity.Refresh();
            var tickets = entity.DeserializeTickets<BmMatchmakerTicket>();
            tickets[0].LastPollAt = DateTime.UtcNow.AddSeconds(-30);
            entity.SerializeTickets(tickets);
            entity.Save();

            OnFacet<BmMatchmakerFacet>()
                .CallSync<BmMatchEntity>("PollMatchmaker", false);
            
            entity.Refresh();
            tickets = entity.DeserializeTickets<BmMatchmakerTicket>();
            Assert.True(tickets[0].NotPolledForSeconds < 20);
        }

        [Test]
        public void ExpiredNotificationsGetCleanedUp()
        {
            var entity = CreateEntity();
            ActingAs(john);
            OnFacet<BmMatchmakerFacet>().CallSync(
                "JoinMatchmaker", new BmMatchmakerTicket(john.EntityId)
            );
            
            entity.Refresh();
            entity.Notifications.Add(new BasicMatchmakerEntity.Notification {
                playerId = john.EntityId,
                matchId = "some-match-id"
            });
            
            // returns null, coz it gets cleaned up
            Assert.IsNull(OnFacet<BmMatchmakerFacet>()
                .CallSync<BmMatchEntity>("PollMatchmaker", false));
            
            entity.Refresh();
            Assert.IsEmpty(entity.Notifications);
        }

        [Test]
        public void ExpiredMatchesGetCleanedUp()
        {
            // create a new match
            var newMatch = new BmMatchEntity();
            newMatch.Save();
            
            // create an old match
            var oldMatch = new BmMatchEntity();
            oldMatch.Save();

            // hack the old match to be old
            var document = AQL.Query()
                .Return(() => AF.Document(oldMatch.EntityId))
                .Execute()
                .First();
            document["CreatedAt"] = Serializer.ToJson(
                DateTime.UtcNow
                    .AddMinutes(-1 * 60 * 24)
                    .AddSeconds(-1)
            );
            AQL.Query()
                .Replace(() => document).In("entities_BmMatchEntity")
                .Execute();
            oldMatch.Refresh();
            
            // needed for a poll to work
            ActingAs(john);
            OnFacet<BmMatchmakerFacet>().CallSync(
                "JoinMatchmaker", new BmMatchmakerTicket(john.EntityId)
            );
            
            // poll triggers the cleanup
            Assert.IsNull(OnFacet<BmMatchmakerFacet>()
                .CallSync<BmMatchEntity>("PollMatchmaker", false));
            
            // check that the old match has been deleted, but not the new one
            Assert.IsNotNull(
                DB.Find<BmMatchEntity>(newMatch.EntityId)
            );
            Assert.IsNull(
                DB.Find<BmMatchEntity>(oldMatch.EntityId)
            );
        }
    }
}