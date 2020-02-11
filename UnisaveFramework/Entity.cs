using System;
using System.Linq;
using System.Reflection;
using Unisave.Database;
using LightJson;
using Unisave.Arango;
using Unisave.Contracts;
using Unisave.Entities;
using Unisave.Foundation;
using EntityCrawler = Unisave.Entities.EntityCrawler;

namespace Unisave
{
    /// <summary>
    /// Base class for all entities you create
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// ID that uniquely identifies the entity globally
        /// </summary>
        public string EntityId
        {
            get => documentId?.Id;
            set => documentId = DocumentId.Parse(value);
        }

        /// <summary>
        /// Backing field for EntityId and EntityKey
        /// </summary>
        private DocumentId documentId = DocumentId.Null;

        /// <summary>
        /// Key that uniquely identifies the entity within its type
        /// </summary>
        public string EntityKey
        {
            get => documentId?.Key;

            set
            {
                if (documentId == null)
                    documentId = DocumentId.Null;

                documentId.Key = value;
            }
        }
        
        /// <summary>
        /// Revision value for this entity
        /// Used by the database to detect changes
        /// </summary>
        public string EntityRevision { get; set; }

        /// <summary>
        /// When has been the entity created
        /// Has default DateTime value if the entity hasn't been created yet
        /// </summary>
        [X] public DateTime CreatedAt { get; set; } = default(DateTime);

        /// <summary>
        /// Last time the entity has been saved
        /// Has default DateTime value if the entity hasn't been created yet
        /// </summary>
        [X] public DateTime UpdatedAt { get; set; } = default(DateTime);
        
        /// <summary>
        /// Access to the underlying arango document attributes
        /// </summary>
        /// <param name="attributeName">Name of the attribute to access</param>
        public JsonValue this[string attributeName]
        {
            get
            {
                if (attributeName == "$type")
                    return EntityUtils.GetEntityType(GetType());
                
                if (attributeName == "_id") return EntityId;
                if (attributeName == "_key") return EntityKey;
                if (attributeName == "_rev") return EntityRevision;
                
                return EntityCrawler
                    .GetMappingFor(GetType())
                    .GetAttributeValue(this, attributeName);
            }

            set
            {
                if (attributeName == "_id") EntityId = value;
                if (attributeName == "_key") EntityKey = value;
                if (attributeName == "_rev") EntityRevision = value;

                EntityCrawler
                    .GetMappingFor(GetType())
                    .SetAttributeValue(this, attributeName, value);
            }
        }
        
        /// <summary>
        /// Entity constructor
        /// Has to be parameterless due to the inheritance model
        /// </summary>
        protected Entity()
        {
            // empty for now
        }
        
        /// <summary>
        /// Sets values according to the provided attribute set
        /// </summary>
        public void SetAttributes(JsonObject newAttributes)
        {
            if (newAttributes == null)
                throw new ArgumentNullException();

            foreach (var pair in newAttributes)
                this[pair.Key] = pair.Value;
        }

        /// <summary>
        /// Returns values of entity attributes
        /// </summary>
        public JsonObject GetAttributes()
        {
            var attributes = new JsonObject();

            string[] defaultNames = {
                "$type",
                "_key",
                "_id",
                "_rev"
            };
            
            var names = EntityCrawler
                .GetMappingFor(GetType())
                .GetAttributeNames();
            
            foreach (var name in defaultNames.Concat(names))
                attributes[name] = this[name];

            return attributes;
        }

        
        
        
        
        
        
        // OLD CODE =========================================
        
        
        
        
        

        /// <summary>
        /// Players that own this entity
        /// </summary>
        public EntityOwners Owners { get; private set; } = new EntityOwners(
            isComplete: true // assume a new entity is being created
        );

        /// <summary>
        /// Extracts database entity type from a c# entity type
        /// </summary>
        public static string GetEntityType<T>() where T : Entity
        {
            return GetEntityType(typeof(T));
        }

        /// <summary>
        /// Extracts database entity type from a c# entity type
        /// </summary>
        public static string GetEntityType(Type entityType)
        {
            if (!typeof(Entity).IsAssignableFrom(entityType))
                throw new ArgumentException(
                    "Provided type is not an entity type.",
                    nameof(entityType)
                );

            return entityType.Name;
        }

        /// <summary>
        /// Create or save entity into the database
        /// New id for the entity will be generated and set
        /// </summary>
        public void Save()
        {
            var database = Application.Default.Resolve<IDatabase>();
            
            RawEntity raw = ToRawEntity();
            database.SaveEntity(raw);
            LoadRawEntity(raw, this); // id, updated_at, created_at
        }

        /// <summary>
        /// Pulls the entity from the database again, to get fresh data
        /// </summary>
        public void Refresh()
        {
            var database = Application.Default.Resolve<IDatabase>();
            
            LoadRawEntity(
                database.LoadEntity(EntityId),
                this
            );
        }

        /// <summary>
        /// Pulls fresh entity data from database and locks the entity
        /// for update, meaning that nobody else can modify or lock it.
        ///
        /// This method should be called inside a transaction.
        /// </summary>
        public void RefreshAndLockForUpdate()
        {
            var database = Application.Default.Resolve<IDatabase>();
            
            LoadRawEntity(
                database.LoadEntity(EntityId, "for_update"),
                this
            );
        }
        
        /// <summary>
        /// Pulls fresh entity data from database and locks the entity
        /// in share mode, meaning that nobody else can modify it.
        /// Others however can also lock it in share mode, but not for update.
        ///
        /// This method should be called inside a transaction.
        /// </summary>
        public void RefreshAndLockShared()
        {
            var database = Application.Default.Resolve<IDatabase>();
            
            LoadRawEntity(
                database.LoadEntity(EntityId, "shared"),
                this
            );
        }

        /// <summary>
        /// Delete the entity from database
        /// </summary>
        public bool Delete()
        {
            var database = Application.Default.Resolve<IDatabase>();
            
            bool result = database.DeleteEntity(EntityId);
            
            // deletion clears all the metadata (but not the actual data)
            EntityId = null;

            return result;
        }

        /// <summary>
        /// Converts the entity to a raw representation,
        /// that is used in the lower levels of the framework
        /// </summary>
        protected RawEntity ToRawEntity()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Loads data from raw entity into an entity instance
        /// </summary>
        private static void LoadRawEntity(RawEntity raw, Entity targetInstance)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Create instance of a given entity type in a non-generic way
        /// (needed for deserialization)
        /// </summary>
        public static Entity CreateInstance(Type entityType)
        {
            // check proper parent
            if (!typeof(Entity).IsAssignableFrom(entityType))
                throw new ArgumentException(
                    $"Provided type {entityType} does not " +
                    "inherit from the Entity class."
                );

            // get parameterless constructor
            ConstructorInfo ci = entityType.GetConstructor(new Type[] { });

            if (ci == null)
                throw new ArgumentException(
                    $"Provided entity type {entityType} lacks " +
                    "parameterless constructor."
                );

            // create instance
            Entity entity = (Entity)ci.Invoke(new object[] { });

            return entity;
        }

        /// <summary>
        /// Creates entity from a raw entity instance
        /// </summary>
        public static Entity FromRawEntity(RawEntity raw, Type entityType)
        {
            if (raw == null)
                return null;

            Entity entityInstance = CreateInstance(entityType);
            LoadRawEntity(raw, entityInstance);
            return entityInstance;
        }

        /// <summary>
        /// Creates entity of the provided type
        /// (checks that the type matches)
        /// and returns the newly created instance
        /// </summary>
        public static Entity FromJson(JsonObject json, Type entityType)
        {
            return FromRawEntity(RawEntity.FromJson(json), entityType);
        }

        /// <summary>
        /// Serialize the entity into json
        /// </summary>
        public JsonObject ToJson()
        {
            return ToRawEntity().ToJson();
        }
    }
}
