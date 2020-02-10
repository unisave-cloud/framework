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
    public class ArangoInMemory : IArango
    {
        /// <summary>
        /// All collections in the database
        /// </summary>
        public Dictionary<string, Collection> Collections { get; }
            = new Dictionary<string, Collection>();
        
        public IEnumerable<JsonValue> ExecuteAqlQuery(AqlQuery query)
        {
            var functionRepository = new AqlFunctionRepository();
            functionRepository.RegisterFunctions();
            //functionRepository.Register("COLLECTION", args => Collections[args[0]]);
            var executor = new QueryExecutor(functionRepository);

            return executor.Execute(query);
        }

        public void CreateCollection(string collectionName, CollectionType type)
        {
            if (Collections.ContainsKey(collectionName))
                throw new ArangoException(409, 1207, "duplicate name");
            
            // TODO: store the type
            Collections[collectionName] = new Collection();
        }

        public void DeleteCollection(string collectionName)
        {
            if (!Collections.ContainsKey(collectionName))
                throw new ArangoException(404, 1203,
                    "collection or view not found");
            
            throw new NotImplementedException();
        }
    }
}