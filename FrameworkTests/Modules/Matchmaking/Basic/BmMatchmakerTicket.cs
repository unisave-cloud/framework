using Unisave;
using Unisave.Modules.Matchmaking;

namespace FrameworkTests.Modules.Matchmaking.Basic
{
    public class BmMatchmakerTicket : BasicMatchmakerTicket
    {
        public int someValue;
        
        public BmMatchmakerTicket() { }
        public BmMatchmakerTicket(string playerId) : base(playerId) { }
    }
}