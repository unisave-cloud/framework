using System;
using System.Collections.Generic;
using LightJson;

namespace Unisave.Database
{
    /// <summary>
    /// Interface used to resolve database connection
    /// from the service container.
    ///
    /// Provides the raw interface to the database. All framework features
    /// should be built on top of this interface. How this interface is
    /// implemented is a Unisave internal thing and you shouldn't rely on it,
    /// because it might change - it's not a guaranteed API.
    /// This interface however is.
    /// </summary>
    public interface IDatabase
    {
        /// <summary>
        /// Saves entity into the database, or if the ID is null,
        /// then the entity gets created. The newly generated ID will
        /// be set into the provided entity instance, together with
        /// the timestamps.
        ///
        /// Any pending ownership operations will be executed.
        /// </summary>
        void SaveEntity(RawEntity entity);

        /// <summary>
        /// Loads entity with given ID from the database.
        /// If the entity does not exist, then null is returned.
        ///
        /// It is not guaranteed that all owners will be loaded,
        /// since there might be too many of them.
        /// </summary>
        RawEntity LoadEntity(string id);

        /// <summary>
        /// Iterates over all owners of a given entity.
        /// If the entity does not exist, the iterator will be empty.
        /// </summary>
        /// <param name="entityId">ID of the entity to query</param>
        IEnumerable<string> GetEntityOwners(string entityId);

        /// <summary>
        /// Deletes an entity by it's ID
        /// </summary>
        /// <param name="id">ID of the entity</param>
        /// <returns>
        /// True if the entity was found and deleted,
        /// false if it didn't even exist.
        /// </returns>
        bool DeleteEntity(string id);

        /// <summary>
        /// Get entities of a given type, satisfying the provided query
        /// </summary>
        IEnumerable<RawEntity> QueryEntities(string entityType, EntityQuery query);
    }
}
