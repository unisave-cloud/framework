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

        /// <summary>
        /// Replaces document in a collection with a given key.
        /// Replaces it with a given document value and options.
        /// Returns the new document state after replacement,
        /// </summary>
        /// <param name="collectionName">Collection to update</param>
        /// <param name="key">Key to find the document</param>
        /// <param name="document">New document value</param>
        /// <param name="options">Options of the operation</param>
        /// <returns>The value that ends up being stored</returns>
        JsonObject ReplaceDocument(
            string collectionName,
            string key,
            JsonObject document,
            JsonObject options
        );

        /// <summary>
        /// Removes a document from a collection
        /// </summary>
        /// <param name="collectionName">Collection to remove from</param>
        /// <param name="key">Document to remove</param>
        /// <param name="rev">Revision to check, can be null</param>
        /// <param name="options">Options for the operation</param>
        void RemoveDocument(
            string collectionName,
            string key,
            string rev,
            JsonObject options
        );
    }
}