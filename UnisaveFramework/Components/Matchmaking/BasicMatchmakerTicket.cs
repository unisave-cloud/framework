using System;

namespace Unisave.Components.Matchmaking
{
    /// <summary>
    /// Represents a player waiting for a match
    /// </summary>
    public class BasicMatchmakerTicket
    {
        /// <summary>
        /// Player who created this ticket
        /// </summary>
        public UnisavePlayer Player { get; private set; }
        
        public DateTime InsertedAt { get; private set; }

        /// <summary>
        /// For how many seconds is this ticket inside the matchmaker
        /// </summary>
        public double WaitingForSeconds
            => (DateTime.UtcNow - InsertedAt).TotalSeconds;

        public BasicMatchmakerTicket(UnisavePlayer player)
        {
            Player = player;
            
            // Should get called later when inserted into the matchmaker,
            // but in case it won't, it's called here.
            InsertedNow();
        }

        /// <summary>
        /// Set the time of matchmaker insertion to now
        /// </summary>
        public void InsertedNow()
        {
            InsertedAt = DateTime.UtcNow;
        }
    }
}