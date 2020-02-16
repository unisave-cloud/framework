using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Unisave.Arango.Query;
using Unisave.Contracts;

namespace Unisave.Entities.Query
{
    public class EntityQuery<TEntity> where TEntity : Entity
    {
        /// <summary>
        /// Underlying AQL query
        /// </summary>
        protected AqlQuery Query { get; set; }
        
        /// <summary>
        /// Arango instance to be used for execution
        /// </summary>
        protected IArango Arango { get; }
        
        public EntityQuery(IArango arango)
        {
            Query = new AqlQuery();
            Arango = arango;
        }

        /// <summary>
        /// Filter entities by a predicate expression
        /// </summary>
        public EntityQuery<TEntity> Filter(Expression<Func<TEntity, bool>> e)
        {
            Query.AddFilterOperation(e.Body);
            return this;
        }

        /// <summary>
        /// Get all entities matching the query as an enumerable
        /// </summary>
        public IEnumerable<TEntity> GetEnumerable()
        {
            return Arango
                .ExecuteAqlQuery(Query)
                .Select(
                    document => (TEntity) Entity.FromJson(
                        document,
                        typeof(TEntity)
                    )
                );
        }

        /// <summary>
        /// Get all entities matching the query as a list
        /// </summary>
        public List<TEntity> Get()
        {
            return GetEnumerable().ToList();
        }
    }
}