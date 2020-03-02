using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unisave.Exceptions;

namespace Unisave.Entities
{
    /// <summary>
    /// Utility methods for working with entities
    /// </summary>
    public static class EntityUtils
    {
        /// <summary>
        /// Prefix for entity collection names
        /// </summary>
        public const string CollectionPrefix = "entities_";

        /// <summary>
        /// Builds collection name from entity string type
        /// </summary>
        public static string CollectionFromType(string entityType)
            => CollectionPrefix + entityType;

        /// <summary>
        /// Builds collection name from entity class type
        /// </summary>
        public static string CollectionFromType(Type entityType)
            => CollectionFromType(GetEntityStringType(entityType));

        /// <summary>
        /// Extracts entity string type from a collection name.
        /// Throws on invalid collection name.
        /// </summary>
        public static string TypeFromCollection(string collectionName)
        {
            if (!collectionName.StartsWith(CollectionPrefix)
                || collectionName.Contains("/"))
                throw new ArgumentException(
                    $"Collection name '{collectionName}' has invalid format"
                );
            
            return collectionName.Substring(CollectionPrefix.Length);
        }

        /// <summary>
        /// Builds entity ID from entity string type and key
        /// </summary>
        public static string EntityIdFromParts(
            string entityType,
            string entityKey
        ) => CollectionPrefix + entityType + "/" + entityKey;
        
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
            
            return member.Name;
        }
        
        /// <summary>
        /// Converts entity class type to entity string type
        /// </summary>
        public static string GetEntityStringType(Type type)
        {
            if (!typeof(Entity).IsAssignableFrom(type))
                throw new ArgumentException(
                    "Provided type is not an entity type.",
                    nameof(type)
                );

            return type.Name;
        }

        /// <summary>
        /// Converts entity string type to entity class type
        /// </summary>
        public static Type GetEntityClassType(
            string type, IEnumerable<Type> typesToSearch
        )
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            
            if (typesToSearch == null)
                throw new ArgumentNullException(nameof(typesToSearch));
            
            List<Type> candidates = typesToSearch
                .Where(t => t.Name == type)
                .Where(t => typeof(Entity).IsAssignableFrom(t))
                .Where(t => t != typeof(Entity))
                .ToList();

            if (candidates.Count > 1)
                throw new EntitySearchException(
                    $"Entity type '{type}' is ambiguous. "
                    + "Make sure you don't have two entities with the same name."
                );

            if (candidates.Count == 0)
                throw new EntitySearchException(
                    $"Entity type '{type}' was not found. "
                    + "Make sure your class inherits from the " +
                    $"{nameof(Entity)} class."
                );

            return candidates[0];
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
                throw new ArgumentException(
                    $"Provided entity type {type} lacks " +
                    "parameterless constructor."
                );

            // create instance
            Entity entity = (Entity)ci.Invoke(new object[] { });

            return entity;
        }
    }
}