using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LightJson;
using Unisave.Arango;
using Unisave.Facades;

namespace Unisave.Entities
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
        /// Entity constructor
        /// Has to be parameterless due to the inheritance model
        /// </summary>
        protected Entity()
        {
            // empty for now
        }
        
        #region "Attributes"
        
        /// <summary>
        /// Access to the underlying arango document attributes
        /// </summary>
        /// <param name="attributeName">Name of the attribute to access</param>
        public JsonValue this[string attributeName]
        {
            get
            {
                if (attributeName == "$type")
                    return EntityUtils.GetEntityStringType(GetType());
                
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
        
        #endregion
        
        #region "Serialization"

        /// <summary>
        /// Creates entity from its JSON representation
        /// by having the type provided.
        /// The provided type will be checked to be correct.
        /// </summary>
        public static Entity FromJson(JsonObject json, Type entityType)
        {
            if (json == null)
                return null;

            // verify $type to match Type
            string type = json["$type"].AsString;
            if (type != EntityUtils.GetEntityStringType(entityType))
                throw new ArgumentException(
                    $"Provided JSON entity representation has type '{type}', "
                    + $"but {entityType} was requested in the argument."
                );
            
            Entity entity = EntityUtils.CreateInstance(entityType);
            entity.SetAttributes(json);
            return entity;
        }

        /// <summary>
        /// Creates entity from its JSON representation
        /// by searching for the proper entity C# type
        /// </summary>
        public static Entity FromJson(
            JsonObject json,
            IEnumerable<Type> typesToSearch
        )
        {
            if (json == null)
                return null;
            
            string type = json["$type"].AsString;
            
            if (type == null)
                throw new ArgumentException(
                    "Given JSON entity representation does not contain $type"
                );
            
            return FromJson(
                json,
                EntityUtils.GetEntityClassType(type, typesToSearch)
            );
        }

        /// <summary>
        /// Serialize the entity into json
        /// </summary>
        public JsonObject ToJson()
        {
            return GetAttributes();
        }
        
        #endregion

        #region "Database operations"

        /// <summary>
        /// Obtains the entity manager used to access the database
        /// (normally accessed via the Facade.App instance)
        /// </summary>
        protected virtual EntityManager GetEntityManager()
        {
            return Facade.App.Resolve<EntityManager>();
        }
        
        /// <summary>
        /// Saves the entity state into the database
        /// </summary>
        /// <param name="carefully">Whether to check document revisions</param>
        public virtual void Save(bool carefully = false)
        {
            if (EntityId == null)
                Insert();
            else
                Update(carefully);
        }
        
        /// <summary>
        /// Saves the entity into the database and performs revision checks
        /// </summary>
        /// <exception cref="EntityRevConflictException">
        /// Entity in the database has been changed while we worked on our copy
        /// </exception>
        public void SaveCarefully() => Save(carefully: true);

        /// <summary>
        /// Inserts the entity into the database
        /// </summary>
        protected virtual void Insert()
        {
            var manager = GetEntityManager();
            var attributes = GetAttributes();
            var newAttributes = manager.Insert(attributes);
            SetAttributes(newAttributes);
        }

        /// <summary>
        /// Updates the entity inside the database
        /// </summary>
        /// <param name="carefully">Whether to check revisions</param>
        protected virtual void Update(bool carefully)
        {
            var manager = GetEntityManager();
            var attributes = GetAttributes();
            var newAttributes = manager.Update(attributes, carefully);
            SetAttributes(newAttributes);
        }
        
        /// <summary>
        /// Pulls the entity from the database again, to get fresh data
        /// </summary>
        public virtual void Refresh()
        {
            // nothing to refresh, it's not even saved yet
            if (EntityId == null)
                return;

            var manager = GetEntityManager();
            var newAttributes = manager.Find(EntityId);
            SetAttributes(newAttributes);
        }
        
        /// <summary>
        /// Delete the entity from database
        /// </summary>
        public virtual void Delete(bool carefully = false)
        {
            // nothing to delete, it's not even saved yet
            if (EntityId == null)
                return;

            var manager = GetEntityManager();
            var attributes = GetAttributes();
            manager.Delete(attributes, carefully);
            
            // clear metadata
            EntityId = null;
        }
        
        #endregion
    }
}
