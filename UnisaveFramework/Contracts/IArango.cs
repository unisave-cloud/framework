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
        List<JsonValue> ExecuteAqlQuery(AqlQuery query);

        /// <summary>
        /// Run a raw AQL query on the database
        /// </summary>
        /// <param name="aql">The query AQL code</param>
        /// <param name="bindParams">Bind params for the query</param>
        /// <returns>List of returned results</returns>
        List<JsonValue> ExecuteRawAqlQuery(string aql, JsonObject bindParams);

        /// <summary>
        /// Create a new collection
        /// </summary>
        void CreateCollection(string collectionName, CollectionType type);

        /// <summary>
        /// Delete an existing collection
        /// </summary>
        void DeleteCollection(string collectionName);

        /// <summary>
        /// Creates an index on a collection
        /// </summary>
        /// <param name="collectionName">Collection name</param>
        /// <param name="indexType">Index type</param>
        /// <param name="fields">Field names to put the index on</param>
        /// <param name="otherProperties">
        /// Other properties depending on the index type.
        /// Will be added to the HTTP request at the root level,
        /// but "type" and "fields" will be overriden by the previous arguments.
        /// </param>
        void CreateIndex(
            string collectionName,
            string indexType,
            string[] fields,
            JsonObject otherProperties = null
        );
    }
}