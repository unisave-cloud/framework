using System;
using System.Collections.Generic;
using System.Linq;
using LightJson;

namespace Unisave.Arango.Database
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

        /// <summary>
        /// This method automatically registers functions provided by the repo.
        /// Make sure to invoke it after construction
        /// </summary>
        public virtual void RegisterFunctions()
        {
            Register("CONCAT", AfConcat);
        }
        
        ///////////////////////////////////////
        // Implementation of basic functions //
        ///////////////////////////////////////

        public static JsonValue AfConcat(JsonValue[] arguments)
        {
            return string.Concat(arguments.Select(a => a.AsString));
        }
    }
}