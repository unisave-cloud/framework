using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
        /// If not and the information is not complete, exception is thrown.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public bool Contains(UnisavePlayer player)
        {
            return OwnerIds.Contains(player.Id);
        }
        
        /// <summary>
        /// Like Contains, but returns null instead of an exception.
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