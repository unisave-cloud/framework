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
        /// Returns a collection by name or throws and arango exception
        /// </summary>
        /// <exception cref="ArangoException"></exception>
        public Collection GetCollection(string collectionName)
        {
            if (!Collections.ContainsKey(collectionName))
                throw new ArangoException(404, 1203,
                    "collection or view not found");

            return Collections[collectionName];
        }
        
        #region "IExecutionDataSource interface"

        IEnumerable<JsonObject> IExecutionDataSource.GetCollection(
            string collectionName
        ) => GetCollection(collectionName);
        
        public JsonObject GetDocument(string collectionName, string key)
            => GetCollection(collectionName).GetDocument(key);

        public JsonObject InsertDocument(
            string collectionName,
            JsonObject document,
            JsonObject options
        ) => GetCollection(collectionName)
            .InsertDocument(document, options);

        public JsonObject ReplaceDocument(
            string collectionName,
            string key,
            JsonObject document,
            JsonObject options
        ) => GetCollection(collectionName)
            .ReplaceDocument(key, document, options);

        #endregion
        
        #region "IArango interface"
        
        public IEnumerable<JsonValue> ExecuteAqlQuery(AqlQuery query)
        {
            return Executor.Execute(query);
        }

        public Collection CreateCollection(string collectionName, CollectionType type)
        {
            if (Collections.ContainsKey(collectionName))
                throw new ArangoException(409, 1207, "duplicate name");
            
            ArangoUtils.ValidateCollectionName(collectionName);
            
            Collections[collectionName] = new Collection(collectionName, type);

            return Collections[collectionName];
        }

        void IArango.CreateCollection(string collectionName, CollectionType type)
            => CreateCollection(collectionName, type);

        public void DeleteCollection(string collectionName)
        {
            if (!Collections.ContainsKey(collectionName))
                throw new ArangoException(404, 1203,
                    "collection or view not found");

            Collections.Remove(collectionName);
        }
        
        #endregion
    }
}