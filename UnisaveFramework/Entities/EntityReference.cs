using System;
using Unisave.Arango;
using Unisave.Facades;
using Unisave.Serialization;

namespace Unisave.Entities
{
    /// <summary>
    /// Reference to some other entity
    /// </summary>
    public struct EntityReference<TTarget>
        where TTarget : Entity
    {
        /// <summary>
        /// ID of the target entity
        /// </summary>
        public string TargetId => targetId;

        /// <summary>
        /// Target id backing field
        /// </summary>
        private readonly string targetId;

        /// <summary>
        /// True if this reference points to null.
        ///
        /// Use this instead of "reference == null" since it is always false.
        /// </summary>
        public bool IsNull => targetId == null;

        static EntityReference()
        {
            // register a serializer for this specific generic version
            Serializer.SetExactTypeSerializer(
                typeof(EntityReference<TTarget>),
                new LambdaTypeSerializer()
                .ToJson((subject) => {
                    return ((EntityReference<TTarget>)subject).TargetId;
                })
                .FromJson((json, type) => {
                    return new EntityReference<TTarget>(json.AsString);
                })
            );
        }
        
        /// <summary>
        /// Create an entity reference already pointing somewhere
        /// </summary>
        public EntityReference(string targetId)
        {
            if (targetId == null)
            {
                this.targetId = null;
                return;
            }

            // validate ID
            DocumentId id;
            try
            {
                id = DocumentId.Parse(targetId);
            }
            catch (ArangoException)
            {
                throw new ArgumentException(
                    $"Given target ID '{targetId}' is not a valid ID"
                );
            }

            // validate entity type
            string given = EntityUtils.TypeFromCollection(id.Collection);
            string shouldBe = EntityUtils.GetEntityStringType(typeof(TTarget));
            if (given != shouldBe)
                throw new ArgumentException(
                    $"Given target ID '{targetId}' has invalid entity type.\n" +
                    $"Given: '{given}', expected: '{shouldBe}'"
                );
            
            this.targetId = targetId;
        }

        /// <summary>
        /// Finds and returns the target entity from the database.
        /// Returns null if the target wasn't found or the reference is null.
        /// </summary>
        public TTarget Find()
        {
            if (IsNull)
                return null;

            return DB.Find<TTarget>(TargetId);
        }
        
        // TODO: add FindOrFail variant
        
        /// <summary>
        /// Explicitly converts ID string to an entity reference
        /// </summary>
        public static explicit operator EntityReference<TTarget>(
            string value
        ) => new EntityReference<TTarget>(value);
        
        /// <summary>
        /// Implicitly converts entity to a reference to the entity.
        ///
        /// This implicit cast allows to easily make comparisons with
        /// entities and also checks target entity type.
        /// </summary>
        public static implicit operator EntityReference<TTarget>(
            TTarget value
        ) => new EntityReference<TTarget>(value?.EntityId);
        
        public static bool operator ==(
            EntityReference<TTarget> a,
            EntityReference<TTarget> b
        ) => a.Equals(b);

        public static bool operator !=(
            EntityReference<TTarget> a,
            EntityReference<TTarget> b
        ) => !(a == b);
        
        public override bool Equals(object obj)
        {
            switch (obj)
            {
                case null:
                    return targetId == null;
                
                case EntityReference<TTarget> that:
                    // Two pointers to null, do they point to the same place?
                    // Technically yes, but really they don't. They do not
                    // point to the same actual entity - they point nowhere.
                    //
                    // Also due to the implicit cast, this might be true
                    // if this condition wasn't present:
                    //    entity.Owner = new MyEntity() // assuming Owner = null
                    //
                    // Use this.IsNull for null checking instead.
                    if (that.targetId == null)
                        return false;
                    return targetId == that.targetId;
                
                case string that:
                    return targetId == that;
                
                case TTarget that:
                    if (that.EntityId == null)
                        return false; // cannot point to non-existing entity
                    return targetId == that.EntityId;
            }

            return false;
        }
        
        public override int GetHashCode()
        {
            return targetId?.GetHashCode() ?? 0;
        }

        public override string ToString()
        {
            return typeof(EntityReference<>) + "(" + (targetId ?? "null") + ")";
        }
    }
}