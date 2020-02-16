using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unisave.Contracts;
using Unisave.Entities;
using Unisave.Foundation;
using Unisave.Services;

namespace Unisave.Database
{
    /// <summary>
    /// Extension of EntityOwnerIds that can automatically
    /// pull missing data from the database and also works
    /// with UnisavePlayer instances instead of IDs.
    /// </summary>
    public class EntityOwners : IEnumerable<UnisavePlayer>
    {
        /// <summary>
        /// The underlying owner IDs instance
        /// </summary>
        public EntityOwnerIds OwnerIds { get; }

        /// <summary>
        /// Is the set of known players complete?
        /// Meaning we know there are no missing owners?
        /// </summary>
        public bool IsComplete => OwnerIds.IsComplete;

        /// <summary>
        /// Returns the total number of entity owners
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the information about owners is not complete
        /// </exception>
        public int Count => OwnerIds.Count;

        /// <summary>
        /// Entity, that this instance belongs to
        /// </summary>
        public Entity ParentEntity { get; set; }

        public EntityOwners(bool isComplete)
        {
            OwnerIds = new EntityOwnerIds(isComplete);
        }
        
        public EntityOwners(EntityOwnerIds ownerIds)
        {
            OwnerIds = ownerIds;
        }
        
        /// <summary>
        /// Add new owner of the entity
        /// </summary>
        public void Add(UnisavePlayer player)
        {
            OwnerIds.Add(player.Id);
        }
        
        /// <summary>
        /// Add multiple owners
        /// </summary>
        public void AddRange(IEnumerable<UnisavePlayer> players)
        {
            OwnerIds.AddRange(players.Select(x => x.Id));
        }

        /// <summary>
        /// Remove an owner of the entity
        /// </summary>
        public void Remove(UnisavePlayer player)
        {
            OwnerIds.Remove(player.Id);
        }
        
        /// <summary>
        /// Returns true if the player is one of the owners.
        /// If not and the information is not complete,
        /// the information is fetched from the database. This will obviously
        /// fail when done on the client side. Use TryContains in such case.
        /// </summary>
        public bool Contains(UnisavePlayer player)
        {
            bool? result = OwnerIds.TryContains(player.Id);

            if (result == null)
            {
                if (ParentEntity == null)
                    throw new InvalidOperationException(
                        "Cannot obtain ownership from database, " +
                        "because parent entity is not known."
                    );
                
                var database = Application.Default.Resolve<IDatabase>();
                return database.IsEntityOwner(ParentEntity.EntityId, player.Id);
            }

            return (bool) result;
        }
        
        /// <summary>
        /// Tries to determine whether a player is an owner.
        /// If this information is not known, null will be returned.
        ///
        /// This method (unlike Contains) does not attempt to access database.
        /// </summary>
        public bool? TryContains(UnisavePlayer player)
        {
            return OwnerIds.TryContains(player.Id);
        }
        
        /// <summary>
        /// Returns the set of known owners
        /// </summary>
        public IEnumerable<UnisavePlayer> GetKnownOwners()
        {
            return OwnerIds.GetKnownOwners().Select(
                x => new UnisavePlayer(x)
            );
        }
        
        public IEnumerator<UnisavePlayer> GetEnumerator()
        {
            return OwnerIds.Select(
                id => new UnisavePlayer(id)
            ).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}