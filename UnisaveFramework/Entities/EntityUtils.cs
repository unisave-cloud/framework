using System;
using System.Reflection;

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
            
            // TODO: validate it's a marked member [X]
            
            return member.Name;
        }
    }
}