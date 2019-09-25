using System;
using System.Collections.Generic;
using System.Linq;
using LightJson;

namespace Unisave.Database.Query
{
    /// <summary>
    /// Describes a specific subset of entities of a given type
    /// </summary>
    public class EntityQuery
    {
        /// <summary>
        /// Type of the queried entities
        /// </summary>
        public string entityType;
        
        /// <summary>
        /// What owners do we require the entity to have
        /// </summary>
        public ISet<UnisavePlayer> requiredOwners = new HashSet<UnisavePlayer>();

        /// <summary>
        /// Is the set of required owners exact,
        /// or can it be just a subset of the actual owners?
        /// </summary>
        public bool requireOwnersExactly = true;

        /// <summary>
        /// How many results to skip
        /// NOT IMPLEMENTED IN THE DATABASE
        /// </summary>
        public int skip = 0;
        
        /// <summary>
        /// How many results to take
        /// -1 means all
        /// NOT IMPLEMENTED IN THE DATABASE
        /// </summary>
        public int take = -1;

        /// <summary>
        /// List of all specified where clauses
        /// </summary>
        public List<WhereClause> whereClauses = new List<WhereClause>();

        /// <summary>
        /// Serialize query to json
        /// </summary>
        public JsonObject ToJson()
        {
            return new JsonObject()
                .Add(nameof(entityType), entityType)
                .Add(
                    nameof(requiredOwners),
                    new JsonArray(
                        requiredOwners.Select(x => (JsonValue)x.Id).ToArray()
                    )
                )
                .Add(nameof(requireOwnersExactly), requireOwnersExactly)
                .Add(nameof(skip), skip)
                .Add(nameof(take), take)
                .Add(
                    nameof(whereClauses),
                    new JsonArray(
                        whereClauses.Select(x => (JsonValue)x.ToJson()).ToArray()
                    )
                );
        }

        /// <summary>
        /// Deserialize query from json
        /// </summary>
        public static EntityQuery FromJson(JsonObject json)
        {
            JsonArray whereClausesJson = new JsonArray();
            
            if (json.ContainsKey(nameof(whereClauses)))
                whereClausesJson = json[nameof(whereClauses)].AsJsonArray;

            return new EntityQuery {
                entityType = json[nameof(entityType)].AsString,
                requiredOwners = new HashSet<UnisavePlayer>(
                    json[nameof(requiredOwners)].AsJsonArray.Select(
                        x => new UnisavePlayer(x.AsString)
                    )
                ),
                requireOwnersExactly = json[nameof(requireOwnersExactly)].AsBoolean,
                skip = json[nameof(skip)].AsInteger,
                take = json[nameof(take)].AsInteger,
                whereClauses = whereClausesJson.Select(
                    x => WhereClause.FromJson(x.AsJsonObject)
                ).ToList()
            };
        }
    }
}
