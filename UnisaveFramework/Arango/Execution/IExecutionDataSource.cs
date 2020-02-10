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
        /// Returns collection to be enumerated
        /// </summary>
        IEnumerable<JsonObject> GetCollection(string collectionName);
        
        /// <summary>
        /// Returns document by its collection and key
        /// </summary>
        JsonObject GetDocument(string collectionName, string key);

        /// <summary>
        /// Inserts document into a collection and returns
        /// the actual stored value of the document after insertion.
        /// Options object can be provided as well.
        /// </summary>
        JsonObject InsertDocument(
            string collectionName,
            JsonObject document,
            JsonObject options
        );
    }
}