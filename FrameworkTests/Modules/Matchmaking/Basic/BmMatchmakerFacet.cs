using System.Collections.Generic;
using Unisave.Facades;
using Unisave.Modules.Matchmaking;

namespace FrameworkTests.Modules.Matchmaking.Basic
{
    public class BmMatchmakerFacet : BasicMatchmakerFacet<
        BmPlayerEntity, BmMatchmakerTicket, BmMatchEntity
    >
    {
        public static string matching;
        
        protected override BmPlayerEntity GetPlayer()
        {
            return Auth.GetPlayer<BmPlayerEntity>();
        }
        
        protected override void PrepareNewTicket(BmMatchmakerTicket ticket)
        {
            ticket.someValue = 42;
        }

        protected override void CreateMatches(List<BmMatchmakerTicket> tickets)
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
        
        private void MatchSingles(List<BmMatchmakerTicket> tickets)
        {
            while (tickets.Count >= 1)
            {
                var selectedTickets = tickets.GetRange(index: 0, count: 1);
                var match = new BmMatchEntity {
                    //
                };
                SaveAndStartMatch(selectedTickets, match);
                tickets.RemoveRange(index: 0, count: 1);
            }
        }
        
        private void MatchPairs(List<BmMatchmakerTicket> tickets)
        {
            while (tickets.Count >= 2)
            {
                // get players that will be matched together
                List<BmMatchmakerTicket> selectedTickets
                    = tickets.GetRange(index: 0, count: 2);
                
                // create and initialize the match
                var match = new BmMatchEntity {
                    //
                };

                // save the match, set entity owners and notify the players
                // that they are no longer waiting
                SaveAndStartMatch(selectedTickets, match);
                
                // make sure we don't match those tickets again
                tickets.RemoveRange(index: 0, count: 2);
            }
        }

        private void MatchTwice(List<BmMatchmakerTicket> tickets)
        {
            while (tickets.Count >= 1)
            {
                var selectedTickets = tickets.GetRange(index: 0, count: 1);
                
                var match = new BmMatchEntity {
                    //
                };
                SaveAndStartMatch(selectedTickets, match);
                
                match = new BmMatchEntity {
                    //
                };
                SaveAndStartMatch(selectedTickets, match); // twice!
                
                tickets.RemoveRange(index: 0, count: 1);
            }
        }
        
        private void MatchSaved(List<BmMatchmakerTicket> tickets)
        {
            while (tickets.Count >= 1)
            {
                var selectedTickets = tickets.GetRange(index: 0, count: 1);
                
                var match = new BmMatchEntity {
                    //
                };
                SaveAndStartMatch(selectedTickets, match);
                SaveAndStartMatch(selectedTickets, match); // already saved!
                
                tickets.RemoveRange(index: 0, count: 1);
            }
        }
    }
}