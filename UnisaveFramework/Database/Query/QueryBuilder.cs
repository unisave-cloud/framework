using System;
using System.Collections.Generic;
using System.Linq;
using Unisave.Serialization;
using LightJson;
using Unisave.Contracts;
using Unisave.Services;

namespace Unisave.Database.Query
{
    /// <summary>
    /// Builds an entity query by chaining commands
    /// </summary>
    public class QueryBuilder<TEntity> where TEntity : Entity, new()
    {
        /// <summary>
        /// Database on which to call the query once finished
        /// </summary>
        private readonly IDatabase database;

        /// <summary>
        /// Parameters of the query
        /// </summary>
        private readonly EntityQuery query;

        public QueryBuilder(IDatabase database = null)
        {
            this.database = database;

            query = new EntityQuery {
                entityType = Entity.GetEntityType<TEntity>()
            };
        }

        /// <summary>
        /// Creates new default query builder
        /// (used in the facade)
        /// </summary>
        public static QueryBuilder<TEntity> Create()
        {
            return new QueryBuilder<TEntity>(
                ServiceContainer.Default.Resolve<IDatabase>()
            );
        }

        /// <summary>
        /// Returns the built up query
        /// </summary>
        public EntityQuery GetQuery() => query;

        /// <summary>
        /// Execute the query and return the matched entities as an enumerable
        /// </summary>
        public IEnumerable<TEntity> GetEnumerable()
        {
            if (database == null)
                throw new InvalidOperationException(
                    $"Cannot execute the query, because no {nameof(IDatabase)} instance is set."
                );

            return database
                .QueryEntities(query)
                .Select(r => (TEntity)Entity.FromRawEntity(r, typeof(TEntity)));
        }

        /// <summary>
        /// Execute the query and return the matched entities in a list
        /// </summary>
        public List<TEntity> Get() => GetEnumerable().ToList();

        /// <summary>
        /// Execute the query and take only the first metching entity, or null
        /// </summary>
        public TEntity First()
        {
            query.take = 1;
            return GetEnumerable().FirstOrDefault();
        }

        /// <summary>
        /// Finds an entity by it's id
        /// No "OfPlayer" or "Where" constraints are applied here.
        /// If entity does not exist, null is returned.
        /// </summary>
        public TEntity Find(string entityId)
        {
            return (TEntity)Entity.FromRawEntity(
                database.LoadEntity(entityId),
                typeof(TEntity)
            );
        }

        /// <summary>
        /// Focus only on entities, that are owned by the given player
        /// </summary>
        public QueryBuilder<TEntity> OfPlayer(UnisavePlayer player)
        {
            query.requiredOwners.Add(player);

            return this;
        }

        /// <summary>
        /// Focus only on entities, that are owned by all the given players
        /// </summary>
        public QueryBuilder<TEntity> OfPlayers(IEnumerable<UnisavePlayer> players)
        {
            foreach (UnisavePlayer player in players)
                query.requiredOwners.Add(player);

            return this;
        }

        /// <summary>
        /// Allow the target entities to be owned not only by the specified players,
        /// but also by any other players
        /// </summary>
        public QueryBuilder<TEntity> AndOthers()
        {
            query.requireOwnersExactly = false;
            
            return this;
        }

        ///////////////////
        // Where clauses //
        ///////////////////
        
        /// <summary>
        /// Filters entities by data
        /// </summary>
        /// <param name="jsonPath">Path to the target data item</param>
        /// <param name="value">Desired value of the data item</param>
        public QueryBuilder<TEntity> Where(string jsonPath, object value)
        {
            return Where(jsonPath, "=", value);
        }

        /// <summary>
        /// Filters entities by data
        /// </summary>
        /// <param name="jsonPath">Path to the target data item</param>
        /// <param name="op">Operator used to compare the values</param>
        /// <param name="value">Desired value of the data item</param>
        public QueryBuilder<TEntity> Where(string jsonPath, string op, object value)
        {
            return Where(
                JsonPath.Parse(jsonPath),
                op,
                Serializer.ToJson(value),
                WhereClause.BooleanValue.And
            );
        }
        
        /// <summary>
        /// Filters entities by data
        /// </summary>
        /// <param name="jsonPath">Path to the target data item</param>
        /// <param name="op">Operator used to compare the values</param>
        /// <param name="value">Desired value of the data item</param>
        public QueryBuilder<TEntity> OrWhere(string jsonPath, string op, object value)
        {
            return Where(
                JsonPath.Parse(jsonPath),
                op,
                Serializer.ToJson(value),
                WhereClause.BooleanValue.Or
            );
        }

        /// <summary>
        /// Full extension of the basic where clause.
        /// </summary>
        /// <param name="jsonPath">Path to target data item</param>
        /// <param name="op">Operator used for value comparison</param>
        /// <param name="value">Constant value used for comparison</param>
        /// <param name="boolean">Logical relation with previous clause</param>
        /// <returns></returns>
        public QueryBuilder<TEntity> Where(
            JsonPath jsonPath,
            string op,
            JsonValue value,
            WhereClause.BooleanValue boolean
        )
        {
            query.whereClauses.Add(
                new BasicWhereClause(
                    jsonPath, op, value, boolean
                )
            );

            return this;
        }
    }
}
