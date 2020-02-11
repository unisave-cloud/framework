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
            
            // validate it's a marked member [X]
            if (!member.GetCustomAttributes().Any(a => a is XAttribute))
                throw new ArgumentException("Given member is not [X] marked");
            
            // validate it does not start with an underscore
            if (member.Name.StartsWith("_"))
                throw new UnisaveException(
                    "Entity attributes cannot start with an underscore, "
                    + "it's a reserved prefix by the ArangoDB, see: " + member
                );
            
            return member.Name;
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
    }
}