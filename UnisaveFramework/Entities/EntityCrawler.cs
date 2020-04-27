using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unisave.Exceptions;

namespace Unisave.Entities
{
    /// <summary>
    /// Builds and caches mapping between attributes and members of entities
    /// </summary>
    public static class EntityCrawler
    {
        public static readonly Dictionary<Type, AttributeMapping> Cache
            = new Dictionary<Type, AttributeMapping>();

        /// <summary>
        /// Builds attribute mapping for a specific entity type
        /// </summary>
        public static void BuildMappingFor(Type entityType)
        {
            if (!typeof(Entity).IsAssignableFrom(entityType))
                throw new ArgumentException(
                    $"Provided type {entityType} does not inherit " +
                    $"from {typeof(Entity)} type."
                );
            
            AttributeMapping mapping = new AttributeMapping(entityType);
            
            // === go over properties ===
            
            var properties = entityType.GetProperties(
                BindingFlags.Public | BindingFlags.Instance
            );
            
            foreach (PropertyInfo pi in properties)
            {
                // skip those not having both getter and setter
                if (!pi.CanRead || !pi.CanWrite)
                    continue;
                
                // skip those that shouldn't be serialized
                if (pi.GetCustomAttribute<DontSerializeAttribute>() != null)
                    continue;
                
                if (typeof(Entity).IsAssignableFrom(pi.PropertyType))
                    throw new UnisaveException(
                        "Entities cannot contain other entities inside, "
                        + "the logic is not yet implemented inside Unisave. "
                        + "Use a string containing the target entity id instead. "
                        + "I'm planning to add support for this in the future however."
                    );
                
                mapping.Add(pi);
            }
            
            // === store the mapping ===

            Cache[entityType] = mapping;
        }

        /// <summary>
        /// Returns attribute mapping for a specific entity type
        /// </summary>
        public static AttributeMapping GetMappingFor(Type entityType)
        {
            if (!Cache.ContainsKey(entityType))
                BuildMappingFor(entityType);

            return Cache[entityType];
        }
    }
}