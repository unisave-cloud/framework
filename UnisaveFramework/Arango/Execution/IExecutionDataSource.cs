using System.Collections.Generic;
using LightJson;

namespace Unisave.Arango.Execution
{
    /// <summary>
    /// Source of data for a query execution
    /// </summary>
    public interface IExecutionDataSource
    {
        /// <summary>
        /// Returns document by its collection and key
        /// </summary>
        JsonObject GetDocument(string collectionName, string key);
        
        /// <summary>
        /// Returns collection to be enumerated
        /// </summary>
        IEnumerable<JsonObject> GetCollection(string collectionName);
    }
}