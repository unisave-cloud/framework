using System;

namespace Unisave.Entities
{
    /// <summary>
    /// Modifies the database collection name used for an entity class
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Class,
        Inherited = false,
        AllowMultiple = false
    )]
    public class EntityCollectionNameAttribute : Attribute
    {
        public string CollectionName { get; }

        public EntityCollectionNameAttribute(string collectionName)
        {
            if (collectionName == null)
                throw new ArgumentNullException(nameof(collectionName));

            if (collectionName.Length == 0)
                throw new ArgumentException(
                    "Entity collection name cannot be empty.",
                    nameof(collectionName)
                );
            
            CollectionName = collectionName;
        }
    }
}