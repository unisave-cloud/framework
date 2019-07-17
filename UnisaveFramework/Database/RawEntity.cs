using System;
using System.Collections.Generic;
using System.Linq;
using LightJson;
using Unisave.Serialization;

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

        /// <summary>
        /// Timestamp of entity creation
        /// </summary>
        public DateTime createdAt;

        /// <summary>
        /// Timestamp of the last time the entity was updated
        /// </summary>
        public DateTime updatedAt;

        /// <summary>
        /// Converts the entity to a json object
        /// </summary>
        public JsonObject ToJson()
        {
            var json = new JsonObject();

            json.Add(nameof(id), id);
            json.Add(nameof(type), type);
            json.Add(nameof(ownerIds), new JsonArray(ownerIds.Select(x => (JsonValue)x).ToArray()));
            json.Add(nameof(data), data);
            json.Add(nameof(createdAt), createdAt.ToString(SerializationParams.DateTimeFormat));
            json.Add(nameof(updatedAt), updatedAt.ToString(SerializationParams.DateTimeFormat));

            return json;
        }

        /// <summary>
        /// Load raw entity from it's json serialized form
        /// </summary>
        public static RawEntity FromJson(JsonObject json)
        {
            return new RawEntity {
                id = json[nameof(id)].AsString,
                type = json[nameof(type)].AsString,
                ownerIds = new HashSet<string>(json[nameof(ownerIds)].AsJsonArray.Select(x => (string)x)),
                data = json[nameof(data)],
                createdAt = DateTime.Parse(json[nameof(createdAt)].AsString),
                updatedAt = DateTime.Parse(json[nameof(updatedAt)].AsString)
            };
        }
    }
}
