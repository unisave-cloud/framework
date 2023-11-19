using System;
using LightJson;
using Unisave.Arango;
using Unisave.Facades;
using Unisave.Serialization.Context;
using Unisave.Serialization.Unisave;
using UnityEngine.Scripting;

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
            get => documentId.Id;
            
            set
            {
                var newDocumentId = DocumentId.Parse(value);

                if (newDocumentId.Collection == null)
                    newDocumentId.Collection = EntityUtils.CollectionFromType(
                        this.GetType()
                    );

                if (newDocumentId.Collection != EntityUtils.CollectionFromType(
                    this.GetType()
                ))
                {
                    throw new InvalidOperationException(
                        "Cannot set entity ID to the given value, " +
                        "since the entity type in the ID does not match."
                    );
                }

                documentId = newDocumentId;
            }
        }

        /// <summary>
        /// Backing field for EntityId and EntityKey
        /// </summary>
        [SerializeAs("_id")]
        [Preserve]
        private DocumentId documentId = DocumentId.Null;

        /// <summary>
        /// Key that uniquely identifies the entity within its type
        /// (Internal so that people don't confuse it with EntityId)
        /// (still can be parsed out of the ID)
        /// </summary>
        internal string EntityKey
        {
            get => documentId.Key;

            set
            {
                if (documentId.Collection == null)
                    documentId.Collection = EntityUtils.CollectionFromType(
                        this.GetType()
                    );

                documentId.Key = value;
            }
        }
        
        /// <summary>
        /// Revision value for this entity
        /// Used by the database to detect changes
        /// </summary>
        [SerializeAs("_rev")]
        [Preserve]
        [field: Preserve]
        public string EntityRevision { get; set; }

        /// <summary>
        /// When has been the entity created
        /// Has default DateTime value if the entity hasn't been created yet
        /// </summary>
        [Preserve]
        [field: Preserve]
        public DateTime CreatedAt { get; set; } = default(DateTime);

        /// <summary>
        /// Last time the entity has been saved
        /// Has default DateTime value if the entity hasn't been created yet
        /// </summary>
        [Preserve]
        [field: Preserve]
        public DateTime UpdatedAt { get; set; } = default(DateTime);
        
        /// <summary>
        /// Set to true by the serializer when received
        /// from an untrusted security domain
        /// </summary>
        [DontSerialize]
        [Preserve]
        [field: Preserve]
        internal bool ContainsInsecureData { get; set; }
        
        /// <summary>
        /// Entity constructor
        /// Has to be parameterless due to the inheritance model
        /// </summary>
        protected Entity()
        {
            // empty for now
        }
        
        #region "Mass assignment"

        /// <summary>
        /// Changes fillable field values to the values in the given entity
        /// (creates copies of all the values)
        /// </summary>
        /// <param name="dataEntity"></param>
        public void FillWith(Entity dataEntity)
        {
            FillWith(
                EntitySerializer.GetAttributes(
                    dataEntity,
                    SerializationContext.ServerToServer
                )
            );
        }
        
        /// <summary>
        /// Changes fillable field values to the values in the given JSON object
        /// (performs deserialization)
        /// </summary>
        /// <param name="json"></param>
        public void FillWith(JsonObject json)
        {
            EntitySerializer.SetAttributes(
                this,
                json,
                DeserializationContext.ServerToServer,
                onlyFillables: true
            );
        }

        private void GuardStorageOfInsecureData()
        {
            if (!ContainsInsecureData)
                return;
            
            throw new EntitySecurityException(
                "You cannot perform this operation on the entity as it " +
                "poses a security risk, because the entity has been received " +
                "from an untrusted source.\n" +
                "For more information ask people on the Unisave discord server."
            );
        }
        
        #endregion

        #region "Database operations"

        /// <summary>
        /// Obtains the entity manager used to access the database
        /// (normally accessed via the Facade.App instance)
        /// </summary>
        protected virtual EntityManager GetEntityManager()
        {
            return Facade.Services.Resolve<EntityManager>();
        }
        
        /// <summary>
        /// Saves the entity state into the database
        /// </summary>
        /// <param name="carefully">Whether to check document revisions</param>
        public virtual void Save(bool carefully = false)
        {
            if (!Facade.CanUse)
                throw new InvalidOperationException(
                    "You cannot save an entity from the client. " +
                    "Only the server has access to the database."
                );
        
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
            
            manager.Insert(this);
        }

        /// <summary>
        /// Updates the entity inside the database
        /// </summary>
        /// <param name="carefully">Whether to check revisions</param>
        protected virtual void Update(bool carefully)
        {
            GuardStorageOfInsecureData();
            
            var manager = GetEntityManager();
            
            manager.Update(this);
        }
        
        /// <summary>
        /// Pulls the entity from the database again, to get fresh data
        /// </summary>
        public virtual void Refresh()
        {
            GuardStorageOfInsecureData();
            
            // nothing to refresh, it's not even saved yet
            if (EntityId == null)
                return;
            
            if (!Facade.CanUse)
                throw new InvalidOperationException(
                    "You cannot refresh an entity from the client. " +
                    "Only the server has access to the database."
                );
            
            var manager = GetEntityManager();
            
            JsonObject freshAttributes = manager.FindDocument(EntityId);
            
            EntitySerializer.SetAttributes(
                this,
                freshAttributes,
                DeserializationContext.ServerStorageToServer
            );
        }
        
        /// <summary>
        /// Delete the entity from database
        /// </summary>
        /// <returns>
        /// True if an entity was removed from database,
        /// false if it wasn't there to begin with
        /// </returns>
        public virtual bool Delete(bool carefully = false)
        {
            GuardStorageOfInsecureData();
            
            // nothing to delete, it's not even saved yet
            if (EntityId == null)
                return false;
            
            if (!Facade.CanUse)
                throw new InvalidOperationException(
                    "You cannot delete an entity from the client. " +
                    "Only the server has access to the database."
                );

            var manager = GetEntityManager();
            
            bool databaseWasModified = manager.Delete(this, carefully);
            
            // clear metadata
            EntityId = null;

            return databaseWasModified;
        }
        
        #endregion
    }
}
