using System;
using System.Collections.Generic;
using LightJson;

namespace Unisave.Database
{
    /// <summary>
    /// Holds the raw entity data, free of any artificial constraints and syntactic sugar
    /// Used for the low-level database API
    /// </summary>
    public class RawEntity
    {
        /// <summary>
        /// Id of the entity
        /// Null for entities not-yet stored in the database
        /// </summary>
        public string id;

        /// <summary>
        /// Type of the entity
        /// Type determines the structure of the carried data. This structure are really just some
        /// agreed upon layers of abstraction, using the unisave serialization system
        /// </summary>
        public string type;

        /// <summary>
        /// Set of all players that own this entity.
        /// Entity ownership helps with querying and reasoning about the entities.
        /// </summary>
        public ISet<string> ownerIds = new HashSet<string>();

        /// <summary>
        /// The actual data, this entity contains. Their structure is determined by the type
        /// (so all entities of a given type should have the same structure, but again,
        /// this is just a convention)
        /// </summary>
        public JsonObject data = new JsonObject();
    }
}
