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
        private static void BuildMappingFor(Type entityType)
        {
            if (!typeof(Entity).IsAssignableFrom(entityType))
                throw new ArgumentException(
                    $"Provided type {entityType} does not inherit " +
                    $"from {typeof(Entity)} type."
                );
            
            AttributeMapping mapping = new AttributeMapping(entityType);
            
            // === go over fields and properties ===
            
            var members = entityType.GetMembers(
                BindingFlags.Public | BindingFlags.Instance
            );
            
            foreach (MemberInfo mi in members)
            {
                // iterate only over fields and properties
                FieldInfo fi = mi as FieldInfo;
                PropertyInfo pi = mi as PropertyInfo;
                if (fi == null && pi == null)
                    continue;
                
                // skip members that shouldn't be serialized
                if (mi.GetCustomAttribute<DontSerializeAttribute>() != null)
                    continue;
                
                // skip properties without both a getter and a setter
                if (pi != null)
                    if (!pi.CanRead || !pi.CanWrite)
                        continue;
                
                // entities cannot be inside entities
                if (typeof(Entity).IsAssignableFrom(fi?.FieldType ?? pi.PropertyType))
                    throw new UnisaveException(
                        "Entities cannot contain other entities. "
                        + "Use entity references instead. "
                    );

                mapping.Add(mi);
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