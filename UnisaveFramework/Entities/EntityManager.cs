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
        public const string CollectionPrefix = "entities_";
        
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
            try
            {
                var id = DocumentId.Parse(entityId);
                id.ThrowIfHasNull();

                var entity = arango.ExecuteAqlQuery(new AqlQuery()
                    .Return(() => AF.Document(id.Collection, id.Key))
                ).First().AsJsonObject;

                if (entity == null)
                    return null;

                entity["_type"] = id.Collection.Substring(
                    CollectionPrefix.Length
                );

                return entity;
            }
            catch (ArangoException e) when (e.ErrorNumber == 1203)
            {
                // collection or view not found
                return null;
            }
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
            string type = entity["$type"].AsString;
            
            if (string.IsNullOrEmpty(type))
                throw new ArgumentException("Provided entity has no '$type'");
            
            if (!string.IsNullOrEmpty(entity["_key"]))
                throw new ArgumentException(
                    "Provided entity already exists, so cannot be inserted"
                );
            
            try
            {
                return AttemptInsert(type, entity);
            }
            catch (ArangoException e) when (e.ErrorNumber == 1203)
            {
                // collection not found -> create it
                arango.CreateCollection(
                    CollectionPrefix + type,
                    CollectionType.Document
                );
                
                return AttemptInsert(type, entity);
            }
        }

        /// <summary>
        /// Attempt to insert an entity, leaving any exceptions go
        /// </summary>
        private JsonObject AttemptInsert(string type, JsonObject entity)
        {
            JsonObject document = JsonReader.Parse(entity.ToString());
            
            // don't store entity type
            document.Remove("$type");
            
            // set timestamps
            DateTime now = DateTime.UtcNow;
            document["CreatedAt"] = now;
            document["UpdatedAt"] = now;
            
            var newEntity = arango.ExecuteAqlQuery(new AqlQuery()
                .Insert(document).Into(CollectionPrefix + type)
                .Return("NEW")
            ).First().AsJsonObject;

            // add entity type to the object
            newEntity["$type"] = type;

            return newEntity;
        }
        
        // TODO: save, saveCarefully, delete, deleteCarefully
    }
}