using System;
using System.Linq;
using LightJson;
using LightJson.Serialization;
using Unisave.Arango;
using Unisave.Arango.Expressions;
using Unisave.Arango.Query;
using Unisave.Contracts;

namespace Unisave.Entities
{
    /// <summary>
    /// Provides access to entities on the JsonObject level
    /// (the first abstraction layer, mapping the concept
    /// of entities onto ArangoDB documents and collections)
    /// </summary>
    public class EntityManager
    {
        /// <summary>
        /// Prefix for entity collection names
        /// </summary>
        private const string CollectionPrefix = "entities_";
        
        /// <summary>
        /// The underlying arango database
        /// </summary>
        private IArango arango;
        
        public EntityManager(IArango arango)
        {
            this.arango = arango;
        }

        /// <summary>
        /// Find entity by its ID
        /// </summary>
        public JsonObject Find(string entityId)
        {
            var id = ArangoUtils.ParseDocumentId(entityId);
            
            var entity = arango.ExecuteAqlQuery(new AqlQuery()
                .Return(() => AF.Document(id.collection, id.key))
            ).First().AsJsonObject;

            if (entity == null)
                return null;

            entity["_type"] = id.collection.Substring(CollectionPrefix.Length);

            return entity;
        }

        /// <summary>
        /// Find entity by type and key
        /// </summary>
        public JsonObject Find(string entityType, string entityKey)
            => Find(CollectionPrefix + entityType + "/" + entityKey);

        /// <summary>
        /// Insert a new entity into the database and return the modified entity
        /// </summary>
        public JsonObject Insert(JsonObject entity)
        {
            string type = entity["_type"].AsString;
            
            if (string.IsNullOrEmpty(type))
                throw new ArgumentException("Provided entity has no '_type'");
            
            if (!string.IsNullOrEmpty(entity["_key"]))
                throw new ArgumentException(
                    "Provided entity already exists, so cannot be inserted"
                );

            JsonObject document = JsonReader.Parse(entity.ToString());
            document.Remove("_type");
            
            var newEntity = arango.ExecuteAqlQuery(new AqlQuery()
                .Insert(() => document).Into(CollectionPrefix + type)
                .Return((NEW) => NEW)
            ).First().AsJsonObject;

            newEntity["_type"] = type;

            return newEntity;
        }
        
        // TODO: save, saveCarefully, delete, deleteCarefully
    }
}