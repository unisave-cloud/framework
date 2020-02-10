using System;
using System.Collections.Generic;
using System.Linq;
using LightJson;

namespace Unisave.Arango.Execution
{
    /// <summary>
    /// Contains implementations of AQL functions
    /// </summary>
    public class AqlFunctionRepository
    {
        /// <summary>
        /// References to function implementations
        /// </summary>
        private Dictionary<string, Func<JsonValue[], JsonValue>> functions
            = new Dictionary<string, Func<JsonValue[], JsonValue>>();

        /// <summary>
        /// Source of data for this function repository
        /// </summary>
        private IExecutionDataSource dataSource;

        public AqlFunctionRepository(IExecutionDataSource dataSource)
        {
            this.dataSource = dataSource;
            
            RegisterBasicFunctions();
        }

        public JsonValue EvaluateFunction(string name, JsonValue[] arguments)
        {
            if (name == null || arguments == null)
                throw new ArgumentNullException();
            
            name = name.ToUpper();

            if (!functions.ContainsKey(name))
                throw new QueryExecutionException(
                    $"Unknown function {name}"
                );

            return functions[name].Invoke(arguments);
        }

        /// <summary>
        /// Registers a function into the repository
        /// </summary>
        public AqlFunctionRepository Register(
            string name,
            Func<JsonValue[], JsonValue> implementation
        )
        {
            if (name == null || implementation == null)
                throw new ArgumentNullException();
            
            name = name.ToUpper();

            functions[name] = implementation;

            return this;
        }

        private void RegisterBasicFunctions()
        {
            Register("DOCUMENT", AfDocument);
            Register("COLLECTION", AfCollection);
            
            Register("CONCAT", AfConcat);
        }
        
        ///////////////////////////////////////
        // Implementation of basic functions //
        ///////////////////////////////////////

        public JsonValue AfDocument(JsonValue[] arguments)
        {
            if (arguments.Length == 1)
            {
                var id = ArangoUtils.ParseDocumentId(arguments[0].AsString);
                return dataSource.GetDocument(id.collection, id.key);
            }
            
            if (arguments.Length == 2)
            {
                return dataSource.GetDocument(arguments[0], arguments[1]);
            }
            
            throw new QueryExecutionException(
                "DOCUMENT function expects 1 or 2 arguments"
            );
        }
        
        public JsonValue AfCollection(JsonValue[] arguments)
        {
            if (arguments.Length != 1)
                throw new QueryExecutionException(
                    "COLLECTION function expects exactly 1 argument"
                );
            
            return new JsonArray(
                dataSource
                    .GetCollection(arguments[0].AsString)
                    .Select(o => (JsonValue)o)
                    .ToArray()
            );
        }
        
        public static JsonValue AfConcat(JsonValue[] arguments)
        {
            return string.Concat(arguments.Select(a => a.AsString));
        }
    }
}