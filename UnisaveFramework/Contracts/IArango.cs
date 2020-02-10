using System.Collections.Generic;
using LightJson;
using Unisave.Arango;
using Unisave.Arango.Query;

namespace Unisave.Contracts
{
    /// <summary>
    /// Interface to an arango database
    /// </summary>
    public interface IArango
    {
        /// <summary>
        /// Run an AQL query on the database
        /// </summary>
        IEnumerable<JsonValue> ExecuteAqlQuery(AqlQuery query);

        /// <summary>
        /// Create a new collection
        /// </summary>
        void CreateCollection(string collectionName, CollectionType type);

        /// <summary>
        /// Delete an existing collection
        /// </summary>
        void DeleteCollection(string collectionName);
    }
}