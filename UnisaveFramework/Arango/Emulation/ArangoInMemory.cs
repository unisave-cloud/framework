using System;
using System.Collections.Generic;
using LightJson;
using Unisave.Arango.Execution;
using Unisave.Arango.Query;
using Unisave.Contracts;

namespace Unisave.Arango.Emulation
{
    /// <summary>
    /// Simulates ArangoDB database in memory
    /// </summary>
    [Obsolete("In-memory database stuff will be removed")]
    public class ArangoInMemory : IExecutionDataSource, IArango
    {
        /// <summary>
        /// All collections in the database
        /// </summary>
        public Dictionary<string, Collection> Collections { get; }
            = new Dictionary<string, Collection>();
        
        /// <summary>
        /// Function repository, bound to this database instance
        /// </summary>
        public AqlFunctionRepository FunctionRepository { get; }
        
        /// <summary>
        /// Executor instance used for query execution
        /// </summary>
        public QueryExecutor Executor { get; }

        public ArangoInMemory()
        {
            FunctionRepository = new AqlFunctionRepository(this);
            Executor = new QueryExecutor(this, FunctionRepository);
        }

        /// <summary>
        /// Returns a collection by name or throws an arango exception
        /// </summary>
        /// <exception cref="ArangoException"></exception>
        public Collection GetCollection(string collectionName)
        {
            if (!Collections.ContainsKey(collectionName))
                throw new ArangoException(404, 1203,
                    "collection or view not found");

            return Collections[collectionName];
        }
        
        /// <summary>
        /// Creates new collection of given type or throws an arango exception
        /// </summary>
        /// <exception cref="ArangoException"></exception>
        public Collection CreateCollection(
            string collectionName,
            CollectionType type
        )
        {
            if (Collections.ContainsKey(collectionName))
                throw new ArangoException(409, 1207, "duplicate name");
            
            ArangoUtils.ValidateCollectionName(collectionName);
            
            Collections[collectionName] = new Collection(collectionName, type);

            return Collections[collectionName];
        }
        
        /// <summary>
        /// Deletes a collection with given name or throws an arango exception
        /// </summary>
        /// <exception cref="ArangoException"></exception>
        public void DeleteCollection(string collectionName)
        {
            if (!Collections.ContainsKey(collectionName))
                throw new ArangoException(404, 1203,
                    "collection or view not found");

            Collections.Remove(collectionName);
        }
        
        /// <summary>
        /// Executes an AQL query against the database
        /// </summary>
        public List<JsonValue> ExecuteAqlQuery(AqlQuery query)
        {
            return Executor.Execute(query);
        }

        /// <summary>
        /// Delete entire database content (delete all collections)
        /// </summary>
        public void Clear()
        {
            Collections.Clear();
        }
        
        #region "IArango interface"

        List<JsonValue> IArango.ExecuteAqlQuery(AqlQuery query)
            => ExecuteAqlQuery(query);

        void IArango.CreateCollection(
            string collectionName, CollectionType type
        ) => CreateCollection(collectionName, type);

        void IArango.DeleteCollection(string collectionName)
            => DeleteCollection(collectionName);

        void IArango.CreateIndex(
            string collectionName,
            string indexType,
            string[] fields,
            JsonObject otherProperties
        )
        {
            // do nothing, in-memory arango does not support indices
        }
        
        #endregion

        #region "IExecutionDataSource interface"

        IEnumerable<JsonObject> IExecutionDataSource.GetCollection(
            string collectionName
        ) => GetCollection(collectionName);
        
        JsonObject IExecutionDataSource.GetDocument(
            string collectionName, string key
        ) => GetCollection(collectionName).GetDocument(key);

        JsonObject IExecutionDataSource.InsertDocument(
            string collectionName,
            JsonObject document,
            JsonObject options
        ) => GetCollection(collectionName)
            .InsertDocument(document, options);

        JsonObject IExecutionDataSource.ReplaceDocument(
            string collectionName,
            string key,
            JsonObject document,
            JsonObject options
        ) => GetCollection(collectionName)
            .ReplaceDocument(key, document, options);

        void IExecutionDataSource.RemoveDocument(
            string collectionName,
            string key,
            string rev,
            JsonObject options
        ) => GetCollection(collectionName)
            .RemoveDocument(key, rev, options);

        #endregion
        
        #region "Serialization"

        /// <summary>
        /// What version does the serialized content have
        /// </summary>
        private const int SerializedVersion = 1;

        /// <summary>
        /// Serializes the database to JSON
        /// </summary>
        public JsonObject ToJson()
        {
            var collections = new JsonObject();

            foreach (var pair in Collections)
                collections.Add(pair.Key, pair.Value.ToJson());
            
            return new JsonObject()
                .Add("version", SerializedVersion)
                .Add("collections", collections);
        }

        /// <summary>
        /// Loads the database from JSON
        /// </summary>
        public static ArangoInMemory FromJson(JsonObject json)
        {
            if (json["version"].AsInteger != SerializedVersion)
                throw new ArgumentException(
                    "Given JSON object is not a database serialized into "
                    + "a supported format version"
                );
            
            var arango = new ArangoInMemory();
            
            foreach (var pair in json["collections"].AsJsonObject)
            {
                arango.Collections[pair.Key] = Collection.FromJson(
                    pair.Key,
                    pair.Value
                );
            }

            return arango;
        }

        #endregion
    }
}