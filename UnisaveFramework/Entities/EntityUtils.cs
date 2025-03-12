using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Unisave.Exceptions;

namespace Unisave.Entities
{
    /// <summary>
    /// Utility methods for working with entities
    /// </summary>
    public static class EntityUtils
    {
        /// <summary>
        /// Prefix for default entity collection names
        /// </summary>
        public const string CollectionPrefix = "e_";

        /// <summary>
        /// Builds collection name from entity class type
        /// </summary>
        public static string CollectionFromType(Type entityType)
        {
            // fetch from the cache
            if (collectionCache.TryGetValue(entityType, out string collection))
                return collection;

            // compute and cache
            collection = CollectionFromTypeUncached(entityType);
            collectionCache[entityType] = collection;
            return collection;
        }

        /// <summary>
        /// Caches collection names for entity types
        /// (is thread-safe)
        /// </summary>
        private static readonly IDictionary<Type, string> collectionCache
            = new ConcurrentDictionary<Type, string>();
        
        private static string CollectionFromTypeUncached(Type entityType)
        {
            if (!typeof(Entity).IsAssignableFrom(entityType))
                throw new ArgumentException(
                    "Provided type is not an entity type.",
                    nameof(entityType)
                );
            
            if (entityType.IsAbstract)
                throw new ArgumentException(
                    "Provided type is abstract and abstract entities " +
                    "cannot have collections because they cannot exist, " +
                    "unless inherited.",
                    nameof(entityType)
                );
            
            // attribute to override the collection name
            var attr = entityType.GetCustomAttribute<EntityCollectionNameAttribute>(
                inherit: false
            );

            if (attr != null)
                return attr.CollectionName;
            
            // default collection name
            return CollectionPrefix + entityType.Name;
        }
        
        /// <summary>
        /// Converts MemberInfo of a given entity type to
        /// the corresponding arango document attribute name
        /// </summary>
        public static string MemberInfoToDocumentAttributeName(
            Type entityType,
            MemberInfo member
        )
        {
            if (!typeof(Entity).IsAssignableFrom(entityType))
                throw new ArgumentException(
                    $"Given type {entityType} is not an entity"
                );
            
            // validate it's a serializable member
            if (member.GetCustomAttribute<DontSerializeAttribute>() != null)
                throw new ArgumentException(
                    $"Given member '{member}' is not serializable"
                );
            
            // validate it does not start with an underscore
            if (member.Name.StartsWith("_"))
                throw new UnisaveException(
                    "Entity attributes cannot start with an underscore, "
                    + "it's a reserved prefix by the ArangoDB, see: " + member
                );
            
            // check the [SerializeAs(...)] attribute
            var renaming = member.GetCustomAttribute<SerializeAsAttribute>();
            if (renaming != null)
                return renaming.SerializedName;
            
            // special handling for entity ID and KEY
            if (member.Name == nameof(Entity.EntityId))
                return "_id";
            if (member.Name == nameof(Entity.EntityKey))
                return "_key";
            
            return member.Name;
        }
        
        /// <summary>
        /// Create instance of a given entity via its class type
        /// </summary>
        public static Entity CreateInstance(Type type)
        {
            // check proper parent
            if (!typeof(Entity).IsAssignableFrom(type))
                throw new ArgumentException(
                    $"Provided type {type} does not " +
                    $"inherit from the {typeof(Entity)} class."
                );

            // get parameterless constructor
            ConstructorInfo ci = type.GetConstructor(new Type[] { });

            if (ci == null)
            {
                // If there is no parameterless constructor, then either
                // it is not defined, or IL2CPP has stripped it away.
                // In either case, get at least an empty instance and
                // deserialize into that.
                
                return (Entity) FormatterServices.GetUninitializedObject(
                    type
                );
            }

            // create an instance via the constructor
            return (Entity) ci.Invoke(new object[] { });
        }
    }
}