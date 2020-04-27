using System;
using Unisave.Contracts;
using Unisave.Entities;
using Unisave.Entities.Query;
using Unisave.Exceptions;

namespace Unisave.Facades
{
    /// <summary>
    /// Facade for working with the database
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class DB
    {
        private static IArango GetArango()
        {
            return Facade.App.Resolve<IArango>();
        }

        private static EntityManager GetEntityManager()
        {
            return Facade.App.Resolve<EntityManager>();
        }
        
        /// <summary>
        /// Finds a single entity of given type
        /// </summary>
        public static TEntity Find<TEntity>(string entityId)
            where TEntity : Entity
        {
            return (TEntity) Entity.FromJson(
                GetEntityManager().Find(entityId),
                typeof(TEntity)
            );
        }

        /// <summary>
        /// Take the first (and probably only) entity of a given type.
        /// Returns null if no such entity exists.
        /// </summary>
        public static TEntity First<TEntity>()
            where TEntity : Entity, new()
        {
            return TakeAll<TEntity>().First();
        }
        
        // TODO FirstOrFail variant
        
        /// <summary>
        /// Starts a query on all entities of a given type
        /// </summary>
        public static EntityQuery<TEntity> TakeAll<TEntity>()
            where TEntity : Entity, new()
        {
            return EntityQuery<TEntity>.TakeAll(GetArango());
        }

//        /// <summary>
//        /// Starts a query on all neighbours of a given entity
//        /// via a given relation type
//        /// </summary>
//        public static EntityQuery<TNeighbour> TakeNeighbours<TNeighbour, TRelation>(
//            Entity entity, string direction = "ANY"
//        ) where TNeighbour : Entity where TRelation : Entity
//        {
//            return EntityQuery<TNeighbour>.TakeNeighbours<TRelation>(
//                GetArango(), entity
//            );
//        }

        /// <summary>
        /// Re-runs a closure multiple times, whenever a write-write
        /// conflict is thrown
        /// </summary>
        public static void RetryOnConflict(Action closure, int attempts = 3)
        {
            if (closure == null)
                throw new ArgumentNullException(nameof(closure));
            
            RetryOnConflict<bool>(() => {
                closure.Invoke();
                return true;
            }, attempts);
        }

        /// <summary>
        /// Re-runs a closure multiple times, whenever a write-write
        /// conflict is thrown
        /// </summary>
        public static T RetryOnConflict<T>(Func<T> closure, int attempts = 3)
        {
            if (closure == null)
                throw new ArgumentNullException(nameof(closure));
            
            for (int attempt = 1; attempt <= attempts; attempt++)
            {
                try
                {
                    return closure.Invoke();
                }
                catch (EntityRevConflictException)
                {
                    if (attempt >= attempts)
                        throw;
                }
            }
            
            throw new UnisaveException(
                "This exception should never be thrown."
            );
        }
    }
}