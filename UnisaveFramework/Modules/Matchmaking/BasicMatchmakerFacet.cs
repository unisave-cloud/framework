using System;
using System.Collections.Generic;
using System.Linq;
using Unisave.Contracts;
using Unisave.Modules.Matchmaking.Exceptions;
using Unisave.Database;
using Unisave.Entities;
using Unisave.Facades;
using Unisave.Facets;
using Unisave.Foundation;
using Unisave.Serialization;
using Unisave.Services;

namespace Unisave.Modules.Matchmaking
{
    public abstract class BasicMatchmakerFacet<
        TMatchmakerTicket, TMatchEntity
    > : Facet
        where TMatchmakerTicket : BasicMatchmakerTicket
        where TMatchEntity : Entity, new()
    {
        /// <summary>
        /// After how long an unpolled ticket gets removed
        /// </summary>
        private const int TicketExpirySeconds = 60 * 5;
        
        /// <summary>
        /// After how long an unpolled notification gets removed
        /// </summary>
        private const int NotificationExpirySeconds = 60;

        /// <summary>
        /// After how long is a match expired and should be removed
        /// </summary>
        private const int MatchExpiryMinutes = 60 * 24;
        
        /// <summary>
        /// Holds the entity while we perform operations on it.
        /// Has to be assigned manually in each public facet method.
        /// </summary>
        private BasicMatchmakerEntity entity;

        /// <summary>
        /// Tickets matched during single matchmaking round
        /// </summary>
        private List<TMatchmakerTicket> matchedTickets;
        
        /// <summary>
        /// Returns some name that identifies this matchmaker if multiple
        /// matchmakers are defined.
        ///
        /// This default implementation returns the type name.
        /// </summary>
        public virtual string GetMatchmakerName()
        {
            return GetType().Name;
        }

        /// <summary>
        /// Obtains the corresponding matchmaker entity from database
        ///
        /// Handles entity creation and migration
        /// </summary>
        private BasicMatchmakerEntity GetEntity()
        {
            // try to load the entity
            var e = GetEntity<BasicMatchmakerEntity>
                .Where("MatchmakerName", GetMatchmakerName())
                .First();
            
            // delete deprecated entity
            if (e != null && e.Version != BasicMatchmakerEntity.CurrentVersion)
            {
                e.Delete();
                e = null;
            }

            // create new entity
            if (e == null)
            {
                e = new BasicMatchmakerEntity {
                    MatchmakerName = GetMatchmakerName()
                };
                e.Save();
            }
            
            return e;
        }
        
        /// <summary>
        /// Player wants to join this matchmaker and start waiting
        /// </summary>
        /// <param name="ticket">Ticket of the player</param>
        /// <exception cref="ArgumentException">
        /// Ticket owner and facet caller differ
        /// </exception>
        public void JoinMatchmaker(TMatchmakerTicket ticket)
        {
            // null ticket owner gets set to the caller
            if (ticket.Player == null)
                ticket.Player = Caller;
            
            // ticket owner has to match the caller
            if (ticket.Player != Caller)
                throw new ArgumentException(
                    "Ticket belongs to a different player " +
                    "than the one registering it.",
                    nameof(ticket)
                );
            
            PrepareNewTicket(ticket);
            
            entity = GetEntity();

            DB.RetryOnConflict(() => {
                entity.Refresh();
                var tickets = entity.DeserializeTickets<TMatchmakerTicket>();
                
                // if already waiting, perform a re-insert
                tickets.RemoveAll(t => t.Player == ticket.Player);
                
                // if to be notified, remove from notifications
                entity.Notifications.RemoveAll(n => n.player == Caller);

                // add ticket into the queue
                ticket.InsertedNow();
                tickets.Add(ticket);
                
                entity.SerializeTickets(tickets);
                entity.SaveCarefully();
            });
        }

        /// <summary>
        /// Player polls for new status on his/her matching
        /// </summary>
        /// <param name="leave">Player wants to leave the matchmaker</param>
        /// <returns>Null if not matched yet, match entity otherwise</returns>
        /// <exception cref="UnknownPlayerPollingException">
        /// When the matchmaker has no clue why is this player polling
        /// </exception>
        public TMatchEntity PollMatchmaker(bool leave)
        {
            entity = GetEntity();

            TMatchEntity returnedValue = null;

            // first perform cleanup
            DB.RetryOnConflict(() => {
                entity.Refresh();
                CleanUpExpiredItems();
                entity.SaveCarefully();
            });
            
            DB.RetryOnConflict(() => {
                entity.Refresh();
                
                // player not waiting -> throw
                // unless there's a notification for this player
                if (entity.Notifications.All(n => n.player != Caller))
                {
                    var tickets = entity.DeserializeTickets<TMatchmakerTicket>();
                    if (tickets.All(t => t.Player != Caller))
                        throw new UnknownPlayerPollingException(
                            "Polling, but not waiting in ticket queue, " +
                            "nor having a notification prepared."
                        );
                }
                
                // update poll time for this ticket
                {
                    var tickets = entity.DeserializeTickets<TMatchmakerTicket>();
                    var ticket = tickets.FirstOrDefault(t => t.Player == Caller);
                    ticket?.PolledNow();
                    entity.SerializeTickets(tickets);
                }

                // attempt to create matches
                CallCreateMatches();

                // find notification to return
                var notification = entity.Notifications
                    .FirstOrDefault(n => n.player == Caller);
                if (notification != null)
                {
                    var database = Application.Default.Resolve<IDatabase>();
                    var rawEntity = database.LoadEntity(notification.matchId);
                    returnedValue = (TMatchEntity)Entity.FromRawEntity(
                        rawEntity,
                        typeof(TMatchEntity)
                    );
                    
                    entity.Notifications.RemoveAll(n => n.player == Caller);
                }
                
                // stop waiting no match, but leaving requested
                if (leave && returnedValue == null)
                {
                    var tickets = entity.DeserializeTickets<TMatchmakerTicket>();
                    tickets.RemoveAll(t => t.Player == Caller);
                    entity.SerializeTickets(tickets);
                }

                entity.SaveCarefully();
            });

            return returnedValue;
        }

        /// <summary>
        /// Removes expired tickets and notifications
        /// </summary>
        private void CleanUpExpiredItems()
        {
            // tickets
            var tickets = entity.DeserializeTickets<TMatchmakerTicket>();
            tickets.RemoveAll(t => t.Player == null); // should not happen, but
            tickets.RemoveAll(
                t => t.NotPolledForSeconds > TicketExpirySeconds
            );
            entity.SerializeTickets(tickets);

            // notifications
            entity.Notifications.RemoveAll(
                n => (n.createdAt - DateTime.UtcNow)
                     .TotalSeconds > NotificationExpirySeconds
            );
            
            // matches
            CleanUpMatches();
        }

        /// <summary>
        /// Goes through all matches and deletes old ones
        /// </summary>
        protected virtual void CleanUpMatches()
        {
            var matches = GetEntity<TMatchEntity>
                .OfAnyPlayers()
                .GetEnumerable();

            var now = DateTime.UtcNow;

            foreach (var match in matches)
            {
                if ((now - match.CreatedAt).TotalMinutes > MatchExpiryMinutes)
                    match.Delete();
            }
        }

        /// <summary>
        /// This method handles calling and validation of "CreateMatches"
        /// method. Should be called inside a transaction with locked entity.
        /// Entity should be saved afterwards.
        /// </summary>
        private void CallCreateMatches()
        {
            matchedTickets = new List<TMatchmakerTicket>();
            
            var tickets = entity.DeserializeTickets<TMatchmakerTicket>();
            var ticketsPassed = entity.DeserializeTickets<TMatchmakerTicket>();
            
            CreateMatches(ticketsPassed);
            
            // remove matched tickets
            tickets.RemoveAll(
                t => matchedTickets.Any(m => m.Player == t.Player)
            );
            entity.SerializeTickets(tickets);
        }

        /// <summary>
        /// Set match entity owners to ticket owners, save the match
        /// and notify these players that they are no longer waiting
        /// </summary>
        /// <param name="selectedTickets">Tickets matched together</param>
        /// <param name="match">Entity describing the match</param>
        protected void SaveAndStartMatch(
            IEnumerable<TMatchmakerTicket> selectedTickets,
            TMatchEntity match
        )
        {
            if (selectedTickets == null)
                throw new ArgumentNullException(nameof(selectedTickets));
            
            if (selectedTickets == null)
                throw new ArgumentNullException(nameof(match));
            
            // cannot start an already saved match
            if (match.EntityId != null)
                throw new ArgumentException(
                    "Match entity is already saved. " +
                    "It will be saved automatically, don't save it yourself.'",
                    nameof(match)
                );
            
            // cannot start match that has some owners
            if (match.Owners.Count != 0)
                throw new ArgumentException(
                    "Match entity already has some owners. " +
                    "They will be added automatically, don't add them yourself.",
                    nameof(match)
                );
            
            foreach (var ticket in selectedTickets)
            {
                // add owners
                match.Owners.Add(ticket.Player);
                
                // remember matched tickets
                // (check that no ticket gets matched twice)
                if (matchedTickets.Any(t => t.Player == ticket.Player))
                    throw new InvalidOperationException(
                        "Cannot match a ticket twice, for ticket: " +
                        Serializer.ToJson(ticket)
                    );
                matchedTickets.Add(ticket);
            }

            // save match entity
            match.Save();

            foreach (var ticket in selectedTickets)
            {
                entity.Notifications.Add(
                    new BasicMatchmakerEntity.Notification {
                        player = ticket.Player,
                        matchId = match.EntityId
                    });
            }
        }
        
        /// <summary>
        /// Modify or verify ticket data when a new ticket is inserted
        /// </summary>
        /// <param name="ticket">The inserted ticket</param>
        protected virtual void PrepareNewTicket(TMatchmakerTicket ticket)
        {
            // nothing
        }

        /// <summary>
        /// Given a list of tickets in queue (waiting players),
        /// generate matches by calling SaveAndReleaseMatch
        /// </summary>
        /// <param name="tickets">List of waiting players</param>
        protected abstract void CreateMatches(List<TMatchmakerTicket> tickets);
    }
}