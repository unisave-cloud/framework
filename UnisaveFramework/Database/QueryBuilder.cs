﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Unisave.Database
{
    /// <summary>
    /// Builds an entity query by chaining commands
    /// </summary>
    public class QueryBuilder<E> // where E : Entity
    {
        /// <summary>
        /// Database on which to call the query once finished
        /// </summary>
        private IDatabase database;

        /// <summary>
        /// Parameters of the query
        /// </summary>
        private EntityQuery query = new EntityQuery();

        public QueryBuilder(IDatabase database = null)
        {
            this.database = database;
        }

        /// <summary>
        /// Creates new default query buillder
        /// (used in the facade)
        /// </summary>
        public static QueryBuilder<E> Create()
        {
            return new QueryBuilder<E>(UnisaveDatabase.Instance);
        }

        /// <summary>
        /// Returns the built up query
        /// </summary>
        public EntityQuery GetQuery() => query;

        /// <summary>
        /// Execute the query and return the matched entities as an enumerable
        /// </summary>
        public IEnumerable<RawEntity> GetEnumerable() // TODO: return E, not a raw entity
        {
            if (database == null)
                throw new InvalidOperationException(
                    $"Cannot execute the query, because no {nameof(IDatabase)} instance is set."
                );

            return database.QueryEntities(typeof(E).Name, query);
        }

        /// <summary>
        /// Execute the query and return the matched entities in a list
        /// </summary>
        public List<RawEntity> Get() => GetEnumerable().ToList();

        /// <summary>
        /// Execute the query and take only the first metching entity, or null
        /// </summary>
        public RawEntity First()
        {
            query.takeFirstFound = true;
            return GetEnumerable().FirstOrDefault();
        }

        /// <summary>
        /// Focus only on entities, that are owned by the given player
        /// </summary>
        public QueryBuilder<E> OfPlayer(UnisavePlayer player)
        {
            query.requiredOwners.Add(player);

            return this;
        }

        /// <summary>
        /// Focus only on entities, that are owned by all the given players
        /// </summary>
        public QueryBuilder<E> OfPlayers(IEnumerable<UnisavePlayer> players)
        {
            foreach (UnisavePlayer player in players)
                query.requiredOwners.Add(player);

            return this;
        }

        /// <summary>
        /// Allow the target entities to be owned not only by the specified players,
        /// but also by any other players
        /// </summary>
        public QueryBuilder<E> AndOthers()
        {
            query.requireOwnersExactly = false;
            
            return this;
        }
    }
}