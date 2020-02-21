using System;
using Unisave.Entities;

namespace Unisave.Modules.Matchmaking
{
    /// <summary>
    /// Represents a player waiting for a match
    /// </summary>
    public class BasicMatchmakerTicket
    {
        /// <summary>
        /// Player entity id of the player who created this ticket
        /// </summary>
        public string PlayerId { get; set; }
        
        /// <summary>
        /// When was the ticket inserted into the waiting queue
        /// </summary>
        public DateTime InsertedAt { get; set; }
        
        /// <summary>
        /// When was the ticket polled for the last time
        /// (triggers expiration)
        /// </summary>
        public DateTime LastPollAt { get; set; }

        /// <summary>
        /// For how many seconds is this ticket inside the matchmaker
        /// </summary>
        public double WaitingForSeconds
            => (DateTime.UtcNow - InsertedAt).TotalSeconds;
        
        /// <summary>
        /// For how many seconds has the ticket not been polled
        /// </summary>
        public double NotPolledForSeconds
            => (DateTime.UtcNow - LastPollAt).TotalSeconds;

        public BasicMatchmakerTicket()
        {
            // Should get called later when inserted into the matchmaker,
            // but in case it won't, it's called here.
            InsertedNow();
        }
        
        public BasicMatchmakerTicket(string playerId) : this()
        {
            PlayerId = playerId;
        }

        /// <summary>
        /// Set the time of matchmaker insertion to now
        /// </summary>
        public void InsertedNow()
        {
            InsertedAt = DateTime.UtcNow;
            
            PolledNow();
        }

        /// <summary>
        /// Set last poll time to now
        /// </summary>
        public void PolledNow()
        {
            LastPollAt = DateTime.UtcNow;
        }
    }
}