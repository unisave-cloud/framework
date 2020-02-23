using System;
using System.Collections.Generic;
using System.Linq;
using Unisave.Authentication.Middleware;
using Unisave.Modules.Matchmaking.Exceptions;
using Unisave.Entities;
using Unisave.Facades;
using Unisave.Facets;
using Unisave.Serialization;

namespace Unisave.Modules.Matchmaking
{
    [Middleware(typeof(Authenticate))]
    public abstract class BasicMatchmakerFacet<
        TPlayerEntity,
        TMatchmakerTicket,
        TMatchEntity
    > : Facet
        where TPlayerEntity : Entity, new()
        where TMatchmakerTicket : BasicMatchmakerTicket
        where TMatchEntity : BasicMatchEntity<TPlayerEntity>, new()
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
        /// The player who called method on this facet
        /// </summary>
        private TPlayerEntity Caller
        {
            get
            {
                var player = GetPlayer();
                
                if (player == null)
                    throw new InvalidOperationException(
                        $"Method {nameof(GetPlayer)} has returned null"
                    );

                return player;
            }
        }

        /// <summary>
        /// Returns the player that makes the requests
        /// Returned value may not be null, since we need to know
        /// who is waiting in the matchmaker
        /// </summary>
        protected abstract TPlayerEntity GetPlayer();
        
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
            var e = DB.TakeAll<BasicMatchmakerEntity>()
                .Filter(entity => entity.MatchmakerName == GetMatchmakerName())
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
            if (ticket.PlayerId == null)
                ticket.PlayerId = Caller.EntityId;
            
            // ticket owner has to match the caller
            if (ticket.PlayerId != Caller.EntityId)
                throw new ArgumentException(
                    "Ticket belongs to a different player " +
                    "than the one registering it.",
                    nameof(ticket)
                );
            
            PrepareNewTicket(ticket);
            
            entity = GetEntity();

            DB.RetryOnConflict(() => {
                entity.Refresh();
                var tickets = entity
                    .DeserializeTickets<TMatchmakerTicket>();
                
                // if already waiting, perform a re-insert
                tickets.RemoveAll(t => t.PlayerId == ticket.PlayerId);
                
                // if to be notified, remove from notifications
                entity.Notifications.RemoveAll(
                    n => n.playerId == Caller.EntityId
                );

                // add ticket into the queue
                ticket.InsertedNow();
                tickets.Add(ticket);
                
                entity.SerializeTickets(
                    tickets
                );
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
                if (entity.Notifications.All(
                    n => n.playerId != Caller.EntityId
                ))
                {
                    var tickets = entity
                        .DeserializeTickets<TMatchmakerTicket>();
                    
                    if (tickets.All(t => t.PlayerId != Caller.EntityId))
                        throw new UnknownPlayerPollingException(
                            "Polling, but not waiting in ticket queue, " +
                            "nor having a notification prepared."
                        );
                }
                
                // update poll time for this ticket
                {
                    var tickets = entity
                        .DeserializeTickets<TMatchmakerTicket>();
                    var ticket = tickets.FirstOrDefault(
                        t => t.PlayerId == Caller.EntityId
                    );
                    ticket?.PolledNow();
                    entity.SerializeTickets(
                        tickets
                    );
                }

                // attempt to create matches
                CallCreateMatches();

                // find notification to return
                var notification = entity.Notifications
                    .FirstOrDefault(n => n.playerId == Caller.EntityId);
                if (notification != null)
                {
                    returnedValue = DB.Find<TMatchEntity>(notification.matchId);
                    
                    entity.Notifications.RemoveAll(
                        n => n.playerId == Caller.EntityId
                    );
                }
                
                // stop waiting no match, but leaving requested
                if (leave && returnedValue == null)
                {
                    var tickets = entity
                        .DeserializeTickets<TMatchmakerTicket>();
                    tickets.RemoveAll(t => t.PlayerId == Caller.EntityId);
                    entity.SerializeTickets(
                        tickets
                    );
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
            var tickets = entity
                .DeserializeTickets<TMatchmakerTicket>();
            tickets.RemoveAll(t => t.PlayerId == null); // should not happen, but
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
            var matches = DB.TakeAll<TMatchEntity>().GetEnumerable();

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
            
            var tickets = entity
                .DeserializeTickets<TMatchmakerTicket>();
            var ticketsPassed = entity
                .DeserializeTickets<TMatchmakerTicket>();
            
            CreateMatches(ticketsPassed);
            
            // remove matched tickets
            tickets.RemoveAll(
                t => matchedTickets.Any(m => m.PlayerId == t.PlayerId)
            );
            entity.SerializeTickets(tickets);
        }

        /// <summary>
        /// Set match entity participants to ticket owners, save the match
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
            
            // cannot start match that has some participants
            if (match.Participants.Count != 0)
                throw new ArgumentException(
                    "Match entity already has some participants. " +
                    "They will be added automatically, don't add them yourself.",
                    nameof(match)
                );
            
            foreach (var ticket in selectedTickets)
            {
                // add participants
                match.Participants.Add(
                    new EntityReference<TPlayerEntity>(ticket.PlayerId)
                );
                
                // remember matched tickets
                // (check that no ticket gets matched twice)
                if (matchedTickets.Any(t => t.PlayerId == ticket.PlayerId))
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
                        playerId = ticket.PlayerId,
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