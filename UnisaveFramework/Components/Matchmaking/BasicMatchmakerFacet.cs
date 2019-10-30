using System;
using System.Collections.Generic;

namespace Unisave.Components.Matchmaking
{
    public abstract class BasicMatchmakerFacet<
        TMatchmakingTicket, TMatchEntity
    > : Facet
        where TMatchmakingTicket : BasicMatchmakerTicket
        where TMatchEntity : Entity
    {
        /// <summary>
        /// Returns some name that identifies this matchmaker if multiple
        /// matchmakers required.
        ///
        /// This default implementation returns the type name.
        /// </summary>
        public virtual string GetMatchmakerName()
        {
            return GetType().Name;
        }

        /// <summary>
        /// Obtains the corresponding matchmaker entity from database
        /// </summary>
        private BasicMatchmakerEntity GetEntity()
        {
            // try to load the entity
            var entity = GetEntity<BasicMatchmakerEntity>
                .Where("MatchmakerName", GetMatchmakerName())
                .First();
            
            // delete deprecated entity
            if (entity != null
                && entity.Version != BasicMatchmakerEntity.CurrentVersion)
            {
                entity.Delete();
                entity = null;
            }

            // create new entity
            if (entity == null)
            {
                entity = new BasicMatchmakerEntity {
                    MatchmakerName = GetMatchmakerName()
                };
                entity.Save();
            }
            
            return entity;
        }
        
        /// <summary>
        /// Player wants to join this matchmaker
        /// </summary>
        /// <param name="ticket">Ticket of the player</param>
        /// <exception cref="ArgumentException">
        /// Ticket owner and caller differ
        /// </exception>
        public virtual void JoinMatchmaker(TMatchmakingTicket ticket)
        {
            // ticket owner has to match the caller
            if (ticket.Player != Caller)
                throw new ArgumentException(
                    "Ticket belongs to a different player " +
                    "than the one registering it.",
                    nameof(ticket)
                );
            
            var entity = GetEntity();

            DB.Transaction(() => {
                entity.RefreshAndLockForUpdate();
                var tickets = entity.DeserializeTickets<TMatchmakingTicket>();
                
                // if already waiting, perform a re-insert
                tickets.RemoveAll(t => t.Player == ticket.Player);

                // add ticket into the queue
                ticket.InsertedNow();
                tickets.Add(ticket);
                
                entity.SerializeTickets(tickets);
                entity.Save();
            });
        }

        public virtual TMatchEntity PollMatchmaker()
        {
            return null;
        }

        protected void NotifyMatchOwners(TMatchEntity match)
        {
            // Tells each owner of the match entity,
            // that a match has been generated for them.
        }

        /// <summary>
        /// Given a list of tickets in queue, generate matches
        /// </summary>
        protected abstract void CreateMatches(List<TMatchmakingTicket> tickets);
    }
}