using System.Collections.Generic;
using FrameworkTests.TestingUtils;
using NUnit.Framework;
using Unisave;
using Unisave.Components.Matchmaking;
using Unisave.Database;
using Unisave.Database.Query;
using Unisave.Runtime;
using Unisave.Serialization;
using Unisave.Services;

namespace FrameworkTests.Components.Matchmaking.Basic
{
    [TestFixture]
    public class BasicMatchmakerTest
    {
        private InMemoryDatabase database;
        private UnisavePlayer john;
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
            
            // prepare facet calls
            facet = OnFacet<MatchmakerFacet>
                .AsPlayer(john)
                .WithTypes(typeof(MatchmakerFacet));
        }
        
        [Test]
        public void TicketCanBeInserted()
        {
            var ticket = new MatchmakerTicket(john);
            facet.Call("JoinMatchmaker", ticket);

            var entity = GetEntity<BasicMatchmakerEntity>.First();
            UAssert.AreJsonEqual(ticket, entity.Tickets[0]);
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
    }
    
    ///////////////////////////////
    // Matchmaker implementation //
    ///////////////////////////////

    public class MatchEntity : Entity
    {
        
    }

    public class MatchmakerTicket : BasicMatchmakerTicket
    {
        public MatchmakerTicket(UnisavePlayer player) : base(player) { }
    }
    
    public class MatchmakerFacet : BasicMatchmakerFacet<MatchmakerTicket, MatchEntity>
    {
        protected override void CreateMatches(List<MatchmakerTicket> tickets)
        {
            /*
                Your own matchmaker logic goes here.
                This is a dummy matchmaker that just pairs players.
             */
        
            // TODO: replace default matchmaker with your own

            while (tickets.Count >= 2)
            {
                var match = new MatchEntity {
                    // TODO: setup match properties
                };
                match.Owners.Add(tickets[0].Player);
                match.Owners.Add(tickets[1].Player);
                match.Save();
                tickets.RemoveRange(index: 0, count: 2);

                // Tells each owner of the match entity,
                // that a match has been generated for them.
                NotifyMatchOwners(match);
            
                // NOTE TO JIRKA:
                // - verify 2+ owners, verify is saved
                // - verify tickets have been removed properly after all done
            }
        }
    }
}