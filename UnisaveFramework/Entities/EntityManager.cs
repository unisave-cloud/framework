using System;
using System.Linq;
using LightJson;
using LightJson.Serialization;
using Unisave.Arango;
using Unisave.Arango.Expressions;
using Unisave.Arango.Query;
using Unisave.Contracts;
using Unisave.Facades;
using Unisave.Serialization;
using Unisave.Serialization.Context;
using Unisave.Serialization.Unisave;

namespace Unisave.Entities
{
    /// <summary>
    /// Connects entities to the database
    /// </summary>
    public class EntityManager
    {
        /// <summary>
        /// The underlying arango database
        /// </summary>
        private readonly IArango arango;

        /// <summary>
        /// Some logger instance
        /// </summary>
        private readonly ILog log;
        
        public EntityManager(IArango arango, ILog log)
        {
            this.arango = arango
                ?? throw new ArgumentNullException(nameof(arango));
            
            this.log = log
                ?? throw new ArgumentNullException(nameof(log));
        }

        public virtual TEntity Find<TEntity>(string entityId)
            where TEntity : Entity
        {
            return (TEntity) Find(entityId, typeof(TEntity));
        }

        /// <summary>
        /// Find entity by its ID
        /// </summary>
        public virtual Entity Find(string entityId, Type entityType)
        {
            if (!typeof(Entity).IsAssignableFrom(entityType))
                throw new ArgumentException(
                    $"Given type {entityType} is not an entity"
                );
            
            var id = DocumentId.Parse(entityId);
            if (id.Collection != EntityUtils.CollectionFromType(entityType))
                throw new ArgumentException(
                    "Given entity ID does not belong to the given entity type."
                );

            JsonObject document = FindDocument(entityId);

            return (Entity) Serializer.FromJson(
                document,
                entityType,
                DeserializationContext.ServerStorageToServer
            );
        }

        /// <summary>
        /// Find the document of an entity by entity id
        /// </summary>
        internal JsonObject FindDocument(string entityId)
        {
            try
            {
                var id = DocumentId.Parse(entityId);
                id.ThrowIfHasNull();

                return arango.ExecuteAqlQuery(new AqlQuery()
                    .Return(() => AF.Document(id.Collection, id.Key))
                ).First().AsJsonObject;
            }
            catch (ArangoException e) when (e.ErrorNumber == 1203)
            {
                // collection or view not found
                return null;
            }
        }
        
        /// <summary>
        /// Insert a new entity into the database and return the modified entity
        /// </summary>
        public virtual void Insert(Entity entity)
        {
            string type = EntityUtils.GetEntityStringType(entity.GetType());
            
            if (!string.IsNullOrEmpty(entity.EntityKey))
                throw new ArgumentException(
                    "Provided entity has already been inserted"
                );
            
            try
            {
                AttemptInsert(type, entity);
            }
            catch (ArangoException e) when (e.ErrorNumber == 1203)
            {
                // collection not found -> create it
                arango.CreateCollection(
                    EntityUtils.CollectionFromType(type),
                    CollectionType.Document
                );
                
                AttemptInsert(type, entity);
            }
        }

        /// <summary>
        /// Attempt to insert an entity, leaving any exceptions go
        /// </summary>
        private void AttemptInsert(string type, Entity entity)
        {
            JsonObject document = SerializeEntity(entity);
            
            // make sure _key and _rev are not even present
            // (they will be set automatically to null, but arango
            // requires them to not be present at all - null is not enough)
            document.Remove("_key");
            document.Remove("_rev");
            
            // set timestamps
            // (serializer instead of direct setting, because the serializer
            // does not store "Z" (timezone), so deserialization does not
            // correct for timezones and loads it as it is)
            DateTime now = DateTime.UtcNow;
            document["CreatedAt"] = Serializer.ToJson(now);
            document["UpdatedAt"] = Serializer.ToJson(now);
            
            var newAttributes = arango.ExecuteAqlQuery(new AqlQuery()
                .Insert(document).Into(EntityUtils.CollectionFromType(type))
                .Return("NEW")
            ).First().AsJsonObject;

            // update entity attributes
            EntitySerializer.SetAttributes(
                entity,
                newAttributes,
                DeserializationContext.ServerStorageToServer
            );
        }

        /// <summary>
        /// Updates the entity in the database and returns the updated entity
        /// Careful insert checks revisions to detect write-write conflicts
        /// </summary>
        public virtual void Update(
            Entity entity,
            bool carefully = false
        )
        {
            string type = EntityUtils.GetEntityStringType(entity.GetType());

            if (string.IsNullOrEmpty(entity.EntityKey))
                throw new ArgumentException(
                    "Provided entity has not been inserted yet"
                );

            try
            {
                JsonObject oldDocument = FindDocument(entity.EntityId);

                if (oldDocument == null)
                    throw new ArangoException(404, 1202, "document not found");

                JsonObject newDocument = SerializeEntity(entity);

                // set timestamps
                // (serializer instead of direct setting, because the serializer
                // does not store "Z" (timezone), so deserialization does not
                // correct for timezones and loads it as it is)
                newDocument["CreatedAt"] = oldDocument["CreatedAt"];
                newDocument["UpdatedAt"] = Serializer.ToJson(DateTime.UtcNow);

                var newAttributes = arango.ExecuteAqlQuery(new AqlQuery()
                    .Replace(() => newDocument)
                    .CheckRevs(carefully)
                    .In(EntityUtils.CollectionFromType(type))
                    .Return("NEW")
                ).First().AsJsonObject;

                EntitySerializer.SetAttributes(
                    entity,
                    newAttributes,
                    DeserializationContext.ServerStorageToServer
                );
            }
            catch (ArangoException e) when (e.ErrorNumber == 1200)
            {
                // write-write conflict
                // (can occur even without revision checking)

                // We are not careful, so we don't care.
                // We just log a warning that this happened
                // so that it will be noticed.
                if (!carefully)
                {
                    log.Warning(
                        "Entity wasn't saved due to a write-write conflict " +
                        "(other process was saving the same entity at the " +
                        "same time).\nCheck out documentation for the method " +
                        "entity.SaveCarefully() to learn more.",
                        e
                    );
                }
                
                // if we are careful, we throw an exception
                throw new EntityRevConflictException(
                    "Entity has been modified since the last refresh"
                );
            }
            catch (ArangoException e) when (e.ErrorNumber == 1202)
            {
                // document not found
                throw new EntityPersistenceException(
                    "Entity has not yet been inserted, or already deleted"
                );
            }
            catch (ArangoException e) when (e.ErrorNumber == 1203)
            {
                // collection not found
                throw new EntityPersistenceException(
                    "Entity has not yet been inserted, or already deleted"
                );
            }
        }

        /// <summary>
        /// Deletes an entity
        /// Careful deletes check revisions
        /// </summary>
        public virtual void Delete(Entity entity, bool carefully = false)
        {
            string type = EntityUtils.GetEntityStringType(entity.GetType());
            
            if (string.IsNullOrEmpty(entity.EntityKey))
                throw new ArgumentException(
                    "Provided entity has not been inserted yet"
                );
            
            try
            {
                JsonObject document = SerializeEntity(entity);
                
                arango.ExecuteAqlQuery(new AqlQuery()
                    .Remove(() => document)
                    .CheckRevs(carefully)
                    .In(EntityUtils.CollectionFromType(type))
                );
            }
            catch (ArangoException e)
            {
                if (e.ErrorNumber == 1203) // collection not found
                    throw new EntityPersistenceException(
                        "Entity has not yet been inserted, or already deleted"
                    );
                
                if (e.ErrorNumber == 1202) // document not found
                    throw new EntityPersistenceException(
                        "Entity has not yet been inserted, or already deleted"
                    );
                
                if (e.ErrorNumber == 1200) // conflict
                    throw new EntityRevConflictException(
                        "Entity has been modified since the last refresh"
                    );
                
                throw;
            }
        }

        private JsonObject SerializeEntity(Entity entity)
        {
            return Serializer.ToJson(
                entity,
                null,
                SerializationContext.ServerToServerStorage
            );
        }
    }
}