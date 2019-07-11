using System;
using System.Collections.Generic;

namespace Unisave
{
    /// <summary>
    /// Represents the underlying entity database
    /// </summary>
    public interface IDatabase
    {
        /// <summary>
        /// Get entities of a given type satisfying the provided query
        /// </summary>
        //IList<JsonObject> QueryEntities(string entityType, EntityQuery query);

        /// <summary>
        /// Create new entity instance and return it's ID
        /// </summary>
        //string CreateEntity(string entityType, ISet<string> playerIDs, JsonObject data);

        /// <summary>
        /// Save entity data
        /// </summary>
        //void SaveEntity(string id, ISet<string> playerIDs, JsonObject data);

        /// <summary>
        /// Delete an entity
        /// </summary>
        //void DeleteEntity(string id);
    }
}
