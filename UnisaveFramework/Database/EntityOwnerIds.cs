using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LightJson;

namespace Unisave.Database
{
    /// <summary>
    /// Contains list of owner IDs for an entity.
    /// This list may not be complete.
    /// </summary>
    public class EntityOwnerIds : IEnumerable<string>
    {
        /// <summary>
        /// Set of players we know are definitely owners
        /// </summary>
        private readonly ISet<string> knownPlayers;

        /// <summary>
        /// Is the set of known players complete?
        /// Meaning we know there are no missing owners?
        /// </summary>
        public bool IsComplete { get; set; }
        
        /// <summary>
        /// Returns the total number of entity owners
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the information about owners is not complete
        /// </exception>
        public int Count => IsComplete
            ? knownPlayers.Count
            : throw new InvalidOperationException(
                "Total number of owners is unknown."
            );
        
        /// <summary>
        /// Owners that have been added due to user editing
        /// and thus need to be written during save.
        /// </summary>
        private readonly ISet<string> addedPlayers = new HashSet<string>();
        
        /// <summary>
        /// Owners that have been removed due to user editing
        /// and thus need to be written during save.
        /// </summary>
        private readonly ISet<string> removedPlayers = new HashSet<string>();

        public EntityOwnerIds(bool isComplete)
        {
            knownPlayers = new HashSet<string>();
            IsComplete = isComplete;
        }
        
        public EntityOwnerIds(IEnumerable<string> enumerable, bool isComplete)
        {
            knownPlayers = new HashSet<string>(
                enumerable ?? Enumerable.Empty<string>()
            );
            IsComplete = isComplete;
        }
        
        /// <summary>
        /// Add new owner of the entity
        /// </summary>
        public void Add(string playerId)
        {
            if (removedPlayers.Contains(playerId))
                removedPlayers.Remove(playerId);
            else
                addedPlayers.Add(playerId);

            knownPlayers.Add(playerId);
        }

        /// <summary>
        /// Add multiple owners
        /// </summary>
        public void AddRange(IEnumerable<string> players)
        {
            if (players == null)
                return;
            
            foreach (string p in players)
                Add(p);
        }

        /// <summary>
        /// Remove an owner of the entity
        /// </summary>
        public void Remove(string playerId)
        {
            if (addedPlayers.Contains(playerId))
                addedPlayers.Remove(playerId);
            else
                removedPlayers.Add(playerId);

            knownPlayers.Remove(playerId);
        }

        /// <summary>
        /// Returns true if the player is one of the owners.
        /// If not and the information is not complete, exception is thrown.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public bool Contains(string playerId)
        {
            bool? c = TryContains(playerId);
            
            if (c == null)
                throw new InvalidOperationException(
                    "Player is not one of the known players, " +
                    "but the information about owners is not complete."
                );

            return (bool) c;
        }

        /// <summary>
        /// Like Contains, but returns null instead of an exception.
        /// </summary>
        public bool? TryContains(string playerId)
        {
            if (knownPlayers.Contains(playerId))
                return true;

            if (removedPlayers.Contains(playerId))
                return false;

            if (!IsComplete)
                return null;

            return false;
        }

        /// <summary>
        /// Returns the set of known owners
        /// </summary>
        public IEnumerable<string> GetKnownOwners()
        {
            return knownPlayers.AsEnumerable();
        }
        
        /// <summary>
        /// Returns the set of newly added owners
        /// </summary>
        public IEnumerable<string> GetAddedOwners()
        {
            return addedPlayers.AsEnumerable();
        }
        
        /// <summary>
        /// Returns the set of newly removed owners
        /// </summary>
        public IEnumerable<string> GetRemovedOwners()
        {
            return removedPlayers.AsEnumerable();
        }

        /// <summary>
        /// Entity has been saved, now commit all ownership changes
        /// </summary>
        public void CommitChanges()
        {
            addedPlayers.Clear();
            removedPlayers.Clear();
        }
        
        /// <summary>
        /// Serializes the instance to JSON
        /// </summary>
        public JsonObject ToJson()
        {
            return new JsonObject()
                .Add("known_players", new JsonArray(
                    knownPlayers.Select(x => (JsonValue)x).ToArray()
                ))
                .Add("added_players", new JsonArray(
                    addedPlayers.Select(x => (JsonValue)x).ToArray()
                ))
                .Add("removed_players", new JsonArray(
                    removedPlayers.Select(x => (JsonValue)x).ToArray()
                ))
                .Add("is_complete", IsComplete);
        }

        /// <summary>
        /// Loads an instance from it's JSON serialized form
        /// </summary>
        public static EntityOwnerIds FromJson(JsonValue jsonValue)
        {
            var json = jsonValue.AsJsonObject;
            
            var instance = new EntityOwnerIds(
                json?["known_players"].AsJsonArray?.Select(x => x.AsString),
                json?["is_complete"].AsBoolean ?? false
            );
            
            instance.addedPlayers.UnionWith(
                json?["added_players"].AsJsonArray?.Select(x => x.AsString)
                    ?? Enumerable.Empty<string>()
            );
            
            instance.removedPlayers.UnionWith(
                json?["removed_players"].AsJsonArray?.Select(x => x.AsString)
                    ?? Enumerable.Empty<string>()
            );

            return instance;
        }

        public IEnumerator<string> GetEnumerator()
        {
            if (!IsComplete)
                throw new InvalidOperationException(
                    "Total number of owners is unknown."
                );

            return knownPlayers.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}