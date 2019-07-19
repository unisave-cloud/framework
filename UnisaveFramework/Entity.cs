using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unisave.Runtime;
using Unisave.Database;
using LightJson;

namespace Unisave
{
    /// <summary>
    /// Base class for all entities you create
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// Id that uniquely identifies the entity instance
        /// </summary>
        public string EntityId { get; private set; }

        /// <summary>
        /// Players that own this entity
        /// </summary>
        public ISet<UnisavePlayer> Owners { get; private set; } = new HashSet<UnisavePlayer>();

        /// <summary>
        /// When has been the entity created
        /// Has default DateTime value if the entity hasn't been created yet
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// Last time the entity has been saved
        /// Has default DateTime value if the entity hasn't been created yet
        /// </summary>
        public DateTime UpdatedAt { get; private set; }

        /// <summary>
        /// Extracts database entity type from a c# entity type
        /// </summary>
        public static string GetEntityType<E>() where E : Entity
        {
            return GetEntityType(typeof(E));
        }

        /// <summary>
        /// Extracts database entity type from a c# entity type
        /// </summary>
        public static string GetEntityType(Type entityType)
        {
            if (!typeof(Entity).IsAssignableFrom(entityType))
                throw new ArgumentException(nameof(entityType), "Provided type is not an entity type.");

            return entityType.Name;
        }

        /// <summary>
        /// Create or save entity into the database
        /// New id for the entity will be generated and set
        /// </summary>
        public void Save()
        {
            RawEntity raw = ToRawEntity();
            Endpoints.Database.SaveEntity(raw);
            Entity.LoadRawEntity(raw, this); // id, updated_at, created_at
        }

        /// <summary>
        /// Pulls the entity from the database again, to get fresh data
        /// </summary>
        public void Refresh()
        {
            Entity.LoadRawEntity(
                Endpoints.Database.LoadEntity(EntityId),
                this
            );
        }

        /// <summary>
        /// Delete the entity from database
        /// </summary>
        public bool Delete()
        {
            bool result = Endpoints.Database.DeleteEntity(EntityId);
            
            // deletion clears all the metadata (but not the actual data)
            EntityId = null;
            Owners.Clear();
            CreatedAt = default(DateTime);
            UpdatedAt = default(DateTime);

            return result;
        }

        /// <summary>
        /// Converts the entity to a raw representation,
        /// that is used in the lower levels of the framework
        /// </summary>
        protected RawEntity ToRawEntity()
        {
            var crawler = new EntityCrawler(this.GetType());

            return new RawEntity {
                id = EntityId,
                type = Entity.GetEntityType(this.GetType()),
                ownerIds = new HashSet<string>(Owners.Select(x => x.Id)),
                data = crawler.ExtractData(this),
                createdAt = CreatedAt,
                updatedAt = UpdatedAt
            };
        }

        /// <summary>
        /// Loads data from raw entity into an entity instance
        /// </summary>
        private static void LoadRawEntity(RawEntity raw, Entity targetInstance)
        {
            string targetType = Entity.GetEntityType(targetInstance.GetType());
            if (raw.type != targetType)
                throw new ArgumentException(
                    nameof(targetInstance),
                    $"Trying to load raw entity of type {raw.type} into entity instance of type {targetType}."
                );

            var crawler = new EntityCrawler(targetInstance.GetType());

            targetInstance.EntityId = raw.id;
            targetInstance.Owners = new HashSet<UnisavePlayer>(raw.ownerIds.Select(x => new UnisavePlayer(x)));
            crawler.InsertData(targetInstance, raw.data);
            targetInstance.CreatedAt = raw.createdAt;
            targetInstance.UpdatedAt = raw.updatedAt;
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
                    $"Provided type {entityType} does not inherit from the Entity class."
                );

            // get parameterless constructor
            ConstructorInfo ci = entityType.GetConstructor(new Type[] { });

            if (ci == null)
                throw new ArgumentException(
                    $"Provided entity type {entityType} lacks parameterless constructor."
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

            Entity entityInstance = Entity.CreateInstance(entityType);
            Entity.LoadRawEntity(raw, entityInstance);
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
