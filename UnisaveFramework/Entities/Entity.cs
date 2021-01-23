using System;
using LightJson;
using Unisave.Arango;
using Unisave.Facades;
using Unisave.Serialization.Context;
using Unisave.Serialization.Unisave;

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
                    newDocumentId.Collection = EntityUtils.CollectionFromType(this.GetType());

                if (newDocumentId.Collection != EntityUtils.CollectionFromType(this.GetType()))
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
        public string EntityRevision { get; set; }

        /// <summary>
        /// When has been the entity created
        /// Has default DateTime value if the entity hasn't been created yet
        /// </summary>
        public DateTime CreatedAt { get; set; } = default(DateTime);

        /// <summary>
        /// Last time the entity has been saved
        /// Has default DateTime value if the entity hasn't been created yet
        /// </summary>
        public DateTime UpdatedAt { get; set; } = default(DateTime);
        
        /// <summary>
        /// Entity constructor
        /// Has to be parameterless due to the inheritance model
        /// </summary>
        protected Entity()
        {
            // empty for now
        }

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
            if (!Facade.HasApp)
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
            var manager = GetEntityManager();
            
            manager.Update(this);
        }
        
        /// <summary>
        /// Pulls the entity from the database again, to get fresh data
        /// </summary>
        public virtual void Refresh()
        {
            // nothing to refresh, it's not even saved yet
            if (EntityId == null)
                return;
            
            if (!Facade.HasApp)
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
        public virtual void Delete(bool carefully = false)
        {
            // nothing to delete, it's not even saved yet
            if (EntityId == null)
                return;
            
            if (!Facade.HasApp)
                throw new InvalidOperationException(
                    "You cannot delete an entity from the client. " +
                    "Only the server has access to the database."
                );

            var manager = GetEntityManager();
            
            manager.Delete(this, carefully);
            
            // clear metadata
            EntityId = null;
        }
        
        #endregion
    }
}
