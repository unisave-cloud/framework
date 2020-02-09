using System;
using System.Collections.Generic;
using System.Linq;
using LightJson;
using Unisave.Arango.Query;

namespace Unisave.Arango.Database
{
    /// <summary>
    /// Executes queries
    /// </summary>
    public class QueryExecutor
    {
        /// <summary>
        /// Executes an AQL query
        /// </summary>
        public IEnumerable<JsonValue> Execute(AqlQuery query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));
            
            query.ValidateQuery();
            
            var initialFrame = new ExecutionFrame();

            var frameStream = Enumerable.Repeat(initialFrame, 1);

            foreach (AqlOperation operation in query)
            {
                switch (operation)
                {
                    case AqlReturnOperation op:
                        return op.EvaluateInFrameStream(frameStream);
                    
                    default:
                        throw new ArgumentException(
                            "Unknown operation type " + operation
                        );
                }
            }
            
            // there was no return statement
            return Enumerable.Empty<JsonValue>();
        }

        /// <summary>
        /// Executes the query and returns the result as a JSON array
        /// </summary>
        public JsonArray ExecuteToArray(AqlQuery query)
        {
            var array = new JsonArray();

            foreach (JsonValue item in Execute(query))
                array.Add(item);

            return array;
        }
    }

    /// <summary>
    /// One frame of execution (holds values of variables)
    /// </summary>
    public class ExecutionFrame
    {
        
    }
}