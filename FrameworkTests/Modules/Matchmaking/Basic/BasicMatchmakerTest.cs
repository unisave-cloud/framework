using System;
using System.Collections.Generic;
using FrameworkTests.TestingUtils;
using NUnit.Framework;
using Unisave;
using Unisave.Contracts;
using Unisave.Modules.Matchmaking;
using Unisave.Modules.Matchmaking.Exceptions;
using Unisave.Database;
using Unisave.Services;

namespace FrameworkTests.Modules.Matchmaking.Basic
{
    [TestFixture]
    public class BasicMatchmakerTest
    {
        private InMemoryDatabase database;
        private UnisavePlayer john, peter;
        private OnFacet<MatchmakerFacet> facet;
        
        [SetUp]
        public void SetUp()
        {
            database = new InMemoryDatabase();

            // mock database
            ServiceContainer.Default = new ServiceContainer();
            ServiceContainer.Default.Register<IDatabase>(database);
            
            // populate database
            john = new UnisavePlayer(database.AddPlayer("john@doe.com"));
            peter = new UnisavePlayer(database.AddPlayer("peter@doe.com"));
            
            // prepare facet calls
            facet = OnFacet<MatchmakerFacet>
                .AsPlayer(john)
                .WithTypes(typeof(MatchmakerFacet));

            // match pairs by default
            MatchmakerFacet.matching = "pairs";
        }

        private BasicMatchmakerEntity CreateEntity()
        {
            var entity = new BasicMatchmakerEntity {
                MatchmakerName = typeof(MatchmakerFacet).Name
            };
            entity.Save();
            return entity;
        }
        
        [Test]
        public void TicketCanBeInserted()
        {
            var ticket = new MatchmakerTicket(john);
            facet.Call("JoinMatchmaker", ticket);

            // fix ticket preprocessing
            ticket.someValue = 42;

            var entity = GetEntity<BasicMatchmakerEntity>.First();
            UAssert.AreJsonEqual(ticket, entity.Tickets[0]);
        }
        
        [Test]
        public void NullTicketOwnerGetsSetToTheCaller()
        {
            var ticket = new MatchmakerTicket();
            facet.Call("JoinMatchmaker", ticket);

            var entity = GetEntity<BasicMatchmakerEntity>.First();
            Assert.AreEqual(
                john,
                entity.DeserializeTickets<MatchmakerTicket>()[0].Player
            );
        }

        [Test]
        public void TicketPreparationGetsCalled()
        {
            facet.Call("JoinMatchmaker", new MatchmakerTicket());

            var entity = GetEntity<BasicMatchmakerEntity>.First();
            Assert.AreEqual(
                42,
                entity.DeserializeTickets<MatchmakerTicket>()[0].someValue
            );
        }

        [Test]
        public void InsertingAlreadyInsertedTicketDoesNotDuplicateIt()
        {
            // insert once
            var ticket = new MatchmakerTicket(john);
            facet.Call("JoinMatchmaker", ticket);
            
            // insert twice
            facet.Call("JoinMatchmaker", ticket);
            
            // check only once
            var entity = GetEntity<BasicMatchmakerEntity>.First();
            Assert.AreEqual(1, entity.Tickets.Count);
        }

        [Test]
        public void InsertingToBeNotifiedTicketCancelsTheNotification()
        {
            var entity = CreateEntity();
            entity.Notifications.Add(new BasicMatchmakerEntity.Notification {
                player = john,
                matchId = "some-match-id"
            });
            entity.Save();
            
            var ticket = new MatchmakerTicket(john);
            facet.Call("JoinMatchmaker", ticket);
            
            entity.Refresh();
            Assert.IsEmpty(entity.Notifications);
        }

        [Test]
        public void DeprecatedEntityGetsRecreated()
        {
            var entity = new BasicMatchmakerEntity() {
                Version = -1,
                MatchmakerName = typeof(MatchmakerFacet).Name
            };
            entity.Save();
            
            var ticket = new MatchmakerTicket(john);
            facet.Call("JoinMatchmaker", ticket);
            
            var newEntity = GetEntity<BasicMatchmakerEntity>.First();
            Assert.AreNotEqual(entity.EntityId, newEntity.EntityId);
        }

        [Test]
        public void PollingReturnsValueOnlyWhenPlayerShouldBeNotified()
        {
            var entity = CreateEntity();
            // start waiting to prevent exception from being thrown
            facet.Call("JoinMatchmaker", new MatchmakerTicket(john));
            
            var match = facet.Call<MatchEntity>("PollMatchmaker", false);
            Assert.IsNull(match);
            
            match = new MatchEntity();
            match.Save();
            entity.Refresh();
            entity.Notifications.Add(new BasicMatchmakerEntity.Notification {
                player = john,
                matchId = match.EntityId
            });
            entity.Save();
            
            var polledMatch = facet.Call<MatchEntity>("PollMatchmaker", false);
            Assert.IsNotNull(polledMatch);
            Assert.AreEqual(match.EntityId, polledMatch.EntityId);
        }

        [Test]
        public void PollingWhenNotWaitingThrows()
        {
            Assert.Catch<UnknownPlayerPollingException>(() => {
                facet.Call<MatchEntity>("PollMatchmaker", false);
            });
        }

        [Test]
        public void PlayerCanLeaveTheMatchmaker()
        {
            var entity = CreateEntity();
            facet.Call("JoinMatchmaker", new MatchmakerTicket(john));
            
            entity.Refresh();
            Assert.IsNotEmpty(entity.Tickets);
            
            var polledMatch = facet.Call<MatchEntity>("PollMatchmaker", true);
            Assert.IsNull(polledMatch);
            
            entity.Refresh();
            Assert.IsEmpty(entity.Tickets);
        }
        
        [Test]
        public void PlayerMightWantToLeaveTheMatchmakerButBeMatched()
        {
            var entity = CreateEntity();
            facet.Call("JoinMatchmaker", new MatchmakerTicket(john));
            
            entity.Refresh();
            Assert.IsNotEmpty(entity.Tickets);
            
            var match = new MatchEntity();
            match.Save();
            entity.Refresh();
            entity.Notifications.Add(new BasicMatchmakerEntity.Notification {
                player = john,
                matchId = match.EntityId
            });
            entity.Save();
            
            var polledMatch = facet.Call<MatchEntity>("PollMatchmaker", true);
            Assert.IsNotNull(polledMatch);
            Assert.AreEqual(match.EntityId, polledMatch.EntityId);
        }

        [Test]
        public void TestPairingProcess()
        {
            var entity = CreateEntity();
            facet.Call("JoinMatchmaker", new MatchmakerTicket(john));
            
            Assert.IsNull(facet.Call<MatchEntity>("PollMatchmaker", false));
            
            OnFacet<MatchmakerFacet>
                .AsPlayer(peter)
                .WithTypes(typeof(MatchmakerFacet))
                .Call("JoinMatchmaker", new MatchmakerTicket(peter));
            
            // does the matching
            var johnMatch = facet.Call<MatchEntity>("PollMatchmaker", false);
            entity.Refresh();
            Assert.IsEmpty(entity.Tickets); // empties tickets
            Assert.IsNotEmpty(entity.Notifications); // fills notifications
            
            // returns the last notification
            var peterMatch = OnFacet<MatchmakerFacet>
                .AsPlayer(peter)
                .WithTypes(typeof(MatchmakerFacet))
                .Call<MatchEntity>("PollMatchmaker", false);
            
            Assert.NotNull(johnMatch);
            Assert.NotNull(peterMatch);
            
            Assert.AreEqual(johnMatch.EntityId, peterMatch.EntityId);
            
            Assert.IsTrue(johnMatch.Owners.Contains(john));
            Assert.IsTrue(johnMatch.Owners.Contains(peter));
            
            entity.Refresh();
            Assert.IsEmpty(entity.Tickets);
            Assert.IsEmpty(entity.Notifications);
        }

        [Test]
        public void TicketShouldNotBeMatchedTwice()
        {
            MatchmakerFacet.matching = "match-twice";
            
            facet.Call("JoinMatchmaker", new MatchmakerTicket(john));
            
            Assert.Catch<InvalidOperationException>(() => {
                facet.Call<MatchEntity>("PollMatchmaker", false);
            });
        }
        
        [Test]
        public void CannotStartMatchThatIsAlreadySaved()
        {
            MatchmakerFacet.matching = "match-saved";
            
            facet.Call("JoinMatchmaker", new MatchmakerTicket(john));
            
            Assert.Catch<ArgumentException>(() => {
                facet.Call<MatchEntity>("PollMatchmaker", false);
            });
        }

        [Test]
        public void ExpiredTicketsGetCleanedUp()
        {
            var entity = CreateEntity();
            facet.Call("JoinMatchmaker", new MatchmakerTicket(john));
            
            // time warp
            entity.Refresh();
            var tickets = entity.DeserializeTickets<MatchmakerTicket>();
            tickets[0].LastPollAt = DateTime.UtcNow.AddHours(-1);
            entity.SerializeTickets(tickets);
            entity.Save();

            Assert.Catch<UnknownPlayerPollingException>(() => {
                facet.Call<MatchEntity>("PollMatchmaker", false);
            });
            
            entity.Refresh();
            Assert.IsEmpty(entity.Tickets);
        }
        
        [Test]
        public void PollResetsTicketExpirationTime()
        {
            var entity = CreateEntity();
            facet.Call("JoinMatchmaker", new MatchmakerTicket(john));
            
            // time warp
            entity.Refresh();
            var tickets = entity.DeserializeTickets<MatchmakerTicket>();
            tickets[0].LastPollAt = DateTime.UtcNow.AddSeconds(-30);
            entity.SerializeTickets(tickets);
            entity.Save();

            facet.Call<MatchEntity>("PollMatchmaker", false);
            
            entity.Refresh();
            tickets = entity.DeserializeTickets<MatchmakerTicket>();
            Assert.True(tickets[0].NotPolledForSeconds < 20);
        }

        [Test]
        public void ExpiredNotificationsGetCleanedUp()
        {
            var entity = CreateEntity();
            facet.Call("JoinMatchmaker", new MatchmakerTicket(john));
            
            entity.Refresh();
            entity.Notifications.Add(new BasicMatchmakerEntity.Notification {
                player = john,
                matchId = "some-match-id"
            });
            
            // returns null, coz it gets cleaned up
            Assert.IsNull(facet.Call<MatchEntity>("PollMatchmaker", false));
            
            entity.Refresh();
            Assert.IsEmpty(entity.Notifications);
        }

        [Test]
        public void ExpiredMatchesGetCleanedUp()
        {
            // create a new match
            var newMatch = new MatchEntity();
            newMatch.Save();
            
            // create an old match
            var oldMatch = new MatchEntity();
            oldMatch.Save();

            // hack the old match to be old
            database.entities[oldMatch.EntityId].createdAt
                = DateTime.UtcNow.AddDays(-1).AddSeconds(-1);
            oldMatch.Refresh();
            
            // needed for a poll to work
            facet.Call("JoinMatchmaker", new MatchmakerTicket(john));
            
            // poll triggers the cleanup
            Assert.IsNull(facet.Call<MatchEntity>("PollMatchmaker", false));
            
            // check that the old match has been deleted, but not the new one
            Assert.IsNotNull(
                GetEntity<MatchEntity>.OfAnyPlayers().Find(newMatch.EntityId)
            );
            Assert.IsNull(
                GetEntity<MatchEntity>.OfAnyPlayers().Find(oldMatch.EntityId)
            );
        }
    }
    
    ///////////////////////////////
    // Matchmaker implementation //
    ///////////////////////////////

    public class MatchEntity : Entity
    {
        
    }

    public class MatchmakerTicket : BasicMatchmakerTicket
    {
        public int someValue;
        
        public MatchmakerTicket() { }
        public MatchmakerTicket(UnisavePlayer player) : base(player) { }
    }
    
    public class MatchmakerFacet : BasicMatchmakerFacet<MatchmakerTicket, MatchEntity>
    {
        public static string matching;
        
        protected override void PrepareNewTicket(MatchmakerTicket ticket)
        {
            ticket.someValue = 42;
        }

        protected override void CreateMatches(List<MatchmakerTicket> tickets)
        {
            switch (matching)
            {
                case "singles":
                    MatchPairs(tickets);
                    break;
                
                case "pairs":
                    MatchPairs(tickets);
                    break;
                
                case "match-twice":
                    MatchTwice(tickets);
                    break;
                
                case "match-saved":
                    MatchSaved(tickets);
                    break;
            }
        }
        
        private void MatchSingles(List<MatchmakerTicket> tickets)
        {
            while (tickets.Count >= 1)
            {
                var selectedTickets = tickets.GetRange(index: 0, count: 1);
                var match = new MatchEntity {
                    //
                };
                SaveAndStartMatch(selectedTickets, match);
                tickets.RemoveRange(index: 0, count: 1);
            }
        }
        
        private void MatchPairs(List<MatchmakerTicket> tickets)
        {
            while (tickets.Count >= 2)
            {
                // get players that will be matched together
                List<MatchmakerTicket> selectedTickets
                    = tickets.GetRange(index: 0, count: 2);
                
                // create and initialize the match
                var match = new MatchEntity {
                    //
                };

                // save the match, set entity owners and notify the players
                // that they are no longer waiting
                SaveAndStartMatch(selectedTickets, match);
                
                // make sure we don't match those tickets again
                tickets.RemoveRange(index: 0, count: 2);
            }
        }

        private void MatchTwice(List<MatchmakerTicket> tickets)
        {
            while (tickets.Count >= 1)
            {
                var selectedTickets = tickets.GetRange(index: 0, count: 1);
                
                var match = new MatchEntity {
                    //
                };
                SaveAndStartMatch(selectedTickets, match);
                
                match = new MatchEntity {
                    //
                };
                SaveAndStartMatch(selectedTickets, match); // twice!
                
                tickets.RemoveRange(index: 0, count: 1);
            }
        }
        
        private void MatchSaved(List<MatchmakerTicket> tickets)
        {
            while (tickets.Count >= 1)
            {
                var selectedTickets = tickets.GetRange(index: 0, count: 1);
                
                var match = new MatchEntity {
                    //
                };
                SaveAndStartMatch(selectedTickets, match);
                SaveAndStartMatch(selectedTickets, match); // already saved!
                
                tickets.RemoveRange(index: 0, count: 1);
            }
        }
    }
}