using System;
using System.Collections.Generic;
using LightJson;

namespace Unisave.Database
{
    /// <summary>
    /// Represents the underlying entity database and allows the running facet access to it
    /// 
    /// Once the facet starts executing, the connection is already established and is
    /// then teared down, when the facet finishes
    /// 
    /// This is just an interface that is accessible inside a facet. Actual implementation is
    /// inside the UnisaveDatabase class.
    /// </summary>
    public interface IDatabase
    {
        /// <summary>
        /// Saves entity into the database, or if the id is null, then the entity gets created.
        /// The newly generated id will be set into the provided entity instance.
        /// </summary>
        void SaveEntity(RawEntity entity);

        /// <summary>
        /// Loads entity with given id from the database.
        /// If the entity does not exist, then null is returned.
        /// </summary>
        RawEntity LoadEntity(string id);

        /// <summary>
        /// Deletes an entity by it's id
        /// </summary>
        /// <param name="id">Id of the entity</param>
        /// <returns>True if the entity was found and deleted, false if it didn't even exist</returns>
        bool DeleteEntity(string id);

        /// <summary>
        /// Get entities of a given type, satisfying the provided query
        /// </summary>
        IEnumerable<RawEntity> QueryEntities(string entityType, EntityQuery query);
    }
}
