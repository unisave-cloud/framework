using System;
using System.Collections.Generic;
using System.Linq;
using LightJson;
using Unisave;
using Unisave.Contracts;
using Unisave.Database;
using Unisave.Database.Query;
using Unisave.Utils;

namespace FrameworkTests
{
    /// <summary>
    /// Copied emulated database from asset
    /// with removed serialization capabilities
    ///
    /// TODO: put this in memory database into the framework
    /// and build the emulated database on top of it.
    ///
    /// TODO: move database tests accordingly so that they can be run
    /// (into the framework probbably)
    ///
    /// TODO: allow some mocking, like setting custom timestamps
    /// </summary>
    public class InMemoryDatabase : IDatabase
    {
        public class PlayerRecord : IEquatable<PlayerRecord>
        {
            public string id;
            public string email;

            public bool Equals(PlayerRecord that)
            {
                return this.id == that.id;
            }
        }

        /// <summary>
        /// List of all players
        /// </summary>
        private ISet<PlayerRecord> players = new HashSet<PlayerRecord>();

        /// <summary>
        /// List of all entities
        /// </summary>
        public Dictionary<string, RawEntity> entities = new Dictionary<string, RawEntity>();
        // HACK: it's public coz i need to mock timestamps and I plan to refactor this anyways

        /// <summary>
        /// Pairs of [ entity | player ]
        /// </summary>
        private List<Tuple<string, string>> entityOwnerships = new List<Tuple<string, string>>();

        /// <summary>
        /// When true, the database shouldn't be accessed
        /// (to detect client-side db access)
        /// </summary>
        public bool PreventAccess { get; set; } = false;

        /// <summary>
        /// Called after someone accesses and mutates the database via the IDatabase interface
        /// (meaning from inside the emulated server code)
        /// </summary>
        public event OnChangeEventHandler OnChange;

        /// <summary>
        /// Event handler for the AfterInterfaceAccess event
        /// </summary>
        /// <param name="subject">Database that was accessed</param>
        public delegate void OnChangeEventHandler(InMemoryDatabase subject);

        public InMemoryDatabase()
        {
            Clear();
        }

        /// <summary>
        /// Empty the database
        /// </summary>
        public void Clear(bool raiseChangeEvent = false)
        {
            players.Clear();
            entities.Clear();
            entityOwnerships.Clear();

            if (raiseChangeEvent)
                OnChange?.Invoke(this);
        }

        /// <summary>
        /// Enumerate players inside the database
        /// </summary>
        public IEnumerable<PlayerRecord> EnumeratePlayers()
        {
            return players;
        }

        /// <summary>
        /// Enumerate all entities that belong to a single player
        /// </summary>
        public IEnumerable<RawEntity> EnumeratePlayerEntities(string playerId)
        {
            return entities.Values.Where(e => e.ownerIds.Count == 1 && e.ownerIds.First() == playerId);
        }

        /// <summary>
        /// Enumerate all entities that belong to at least two players
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RawEntity> EnumerateSharedEntities()
        {
            return entities.Values.Where(e => e.ownerIds.Count >= 2);
        }

        /// <summary>
        /// Enumerate all entities that belong to at least two players
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RawEntity> EnumerateGameEntities()
        {
            return entities.Values.Where(e => e.ownerIds.Count == 0);
        }

        /// <summary>
        /// Adds a new player into the database and returns it's ID
        /// </summary>
        public string AddPlayer(string email)
        {
            var id = Str.Random(16);

            players.Add(new PlayerRecord {
                id = id,
                email = email
            });

            OnChange?.Invoke(this);

            return id;
        }

        /// <summary>
        /// Remove a player by it's id
        /// </summary>
        public bool RemovePlayer(string id)
        {
            PlayerRecord player = players.Where(r => r.id == id).FirstOrDefault();

            if (player == null)
                return false;

            players.Remove(player);

            OnChange?.Invoke(this);
            
            return true;
        }

        /// <summary>
        /// Checks proper emulation state.
        /// Throws exception on failure
        /// </summary>
        private void GuardClientSide()
        {
            // nothing
        }

        /// <inheritdoc/>
        public void SaveEntity(RawEntity entity)
        {
            GuardClientSide();

            if (entity.id == null)
                InsertEntity(entity);
            else
                UpdateEntity(entity);

            OnChange?.Invoke(this);
        }

        private void InsertEntity(RawEntity entity)
        {
            // without ms
            var now = DateTime.UtcNow;
            now = new DateTime(
                now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second
            );
            
            entity.id = Str.Random(16);
            entity.updatedAt = entity.createdAt = now;

            entities.Add(entity.id, RawEntity.FromJson(entity.ToJson()));

            AddOwners(entity.id, new HashSet<string>(entity.ownerIds.GetKnownOwners()));
            entity.ownerIds.CommitChanges();
        }

        private void UpdateEntity(RawEntity entity)
        {
            // without ms
            var now = DateTime.UtcNow;
            now = new DateTime(
                now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second
            );
            
            entity.updatedAt = now;

            entities[entity.id] = RawEntity.FromJson(entity.ToJson());

            // edit owner set
            ISet<string> ownerIds = new HashSet<string>(GetEntityOwners(entity.id));
            ownerIds.UnionWith(entity.ownerIds.GetAddedOwners());
            foreach (string o in entity.ownerIds.GetRemovedOwners())
                ownerIds.Remove(o);
            entity.ownerIds.CommitChanges();
            
            RemoveAllOwners(entity.id);
            AddOwners(entity.id, ownerIds);
        }

        /// <inheritdoc/>
        public RawEntity LoadEntity(string id, string lockType = null)
        {
            // NOTE: lockType is ignored in emulated database,
            // because no concurrency happens here.
            
            GuardClientSide();

            if (!entities.ContainsKey(id))
                return null;

            var entity = RawEntity.FromJson(entities[id].ToJson());

            var ownerIds = GetEntityOwners(id).ToList();
            entity.ownerIds = new EntityOwnerIds(
                ownerIds.Take(1),
                ownerIds.Count < 2
            );

            return entity;
        }

        /// <inheritdoc/>
        IEnumerable<string> IDatabase.GetEntityOwners(string entityId)
        {
            return GetEntityOwners(entityId);
        }

        /// <inheritdoc/>
        public bool IsEntityOwner(string entityId, string playerId)
        {
            return GetEntityOwners(entityId).Contains(playerId);
        }

        /// <inheritdoc/>
        public bool DeleteEntity(string id)
        {
            GuardClientSide();

            RemoveAllOwners(id);

            if (!entities.ContainsKey(id))
                return false;

            entities.Remove(id);
            
            OnChange?.Invoke(this);

            return true;
        }

        /// <inheritdoc/>
        public IEnumerable<RawEntity> QueryEntities(EntityQuery query)
        {
            string entityType = query.entityType;
            
            /*
                This implementation is really not the best possible, but I didn't want to waste
                time by overly optimizing a database, that is going to have hundreds of items at most.
                (remember, this is the emulated one, not the real one)
             */

            GuardClientSide();

            // build a set of entities that are owned by all the required players
            HashSet<string> entityIds = null; // null means the entire universe

            foreach (UnisavePlayer player in query.requiredOwners)
            {
                var entityIdsOwnedByThisPlayer = entityOwnerships
                    .Where(t => t.Item2 == player.Id)
                    .Select(t => t.Item1);

                var playerEntityIds = entities
                    .Where(p => p.Value.type == entityType)
                    .Where(p => entityIdsOwnedByThisPlayer.Contains(p.Value.id))
                    .Select(p => p.Value.id);

                if (entityIds == null)
                    entityIds = new HashSet<string>(playerEntityIds);
                else
                    entityIds.IntersectWith(playerEntityIds);
            }

            // players have no effect on entity selection
            if (entityIds == null)
            {
                entityIds = new HashSet<string>(
                    entities.Where(p => p.Value.type == entityType).Select(p => p.Key)
                );
            }

            // load entities
            IEnumerable<RawEntity> loadedEntities = entityIds.Select(id => LoadEntity(id));

            // remove entities that do not match where clauses
            loadedEntities = loadedEntities.Where(e => WhereClausesMatch(e, query));

            // if exact, remove those owned by too many players
            if (query.requireOwnersExactly)
            {
                loadedEntities = loadedEntities.Where(
                    e => GetEntityOwners(e.id).Count() == query.requiredOwners.Count
                );
            }

            // take only a given number or items
            if (query.take != -1)
                return loadedEntities.Take(query.take);

            return loadedEntities;
        }

        /// <summary>
        /// Return true if all where clauses match the entity
        /// </summary>
        private bool WhereClausesMatch(RawEntity e, EntityQuery query)
        {
            return WhereClause.ClausesMatchEntity(query.whereClauses, e);
        }

        /// <summary>
        /// Returns a set of owners of a given entity
        /// </summary>
        public IEnumerable<string> GetEntityOwners(string entityId)
        {
            return entityOwnerships.Where(t => t.Item1 == entityId).Select(t => t.Item2);
        }

        /// <summary>
        /// Removes all owners of an entity
        /// </summary>
        private void RemoveAllOwners(string entityId)
        {
            entityOwnerships.RemoveAll(t => t.Item1 == entityId);
        }

        /// <summary>
        /// Adds given owners to the entity.
        /// Assumes all owners are new
        /// </summary>
        private void AddOwners(string entityId, ISet<string> ownerIds)
        {
            foreach (string ownerId in ownerIds)
                entityOwnerships.Add(new Tuple<string, string>(entityId, ownerId));
        }
        
        //////////////////////////
        // Transaction handling //
        //////////////////////////
        
        /*
         * Dummy implementation that only keeps track of transaction level.
         * Emulated database does not support rollbacks
         * and other transaction features.
         */

        private int transactionLevel = 0;
        
        public void StartTransaction()
        {
            transactionLevel++;
        }

        public void RollbackTransaction()
        {
            if (transactionLevel > 0)
                transactionLevel--;
        }

        public void CommitTransaction()
        {
            if (transactionLevel > 0)
                transactionLevel--;
        }

        public int TransactionLevel()
        {
            return transactionLevel;
        }
    }
}