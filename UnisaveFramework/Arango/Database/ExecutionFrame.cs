using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LightJson;

namespace Unisave.Arango.Database
{
    /// <summary>
    /// One frame of execution (holds values of variables)
    /// Once created, it's immutable and any mutations create mutated copies
    /// </summary>
    public class ExecutionFrame
    {
        public ReadOnlyDictionary<string, JsonValue> Variables { get; }
        public AqlFunctionRepository FunctionRepository { get; }

        public ExecutionFrame(
            AqlFunctionRepository functionRepository,
            Dictionary<string, JsonValue> variables = null
        )
        {
            Variables = new ReadOnlyDictionary<string, JsonValue>(
                variables ?? new Dictionary<string, JsonValue>()
            );
            
            FunctionRepository = functionRepository
                                 ?? throw new ArgumentNullException(nameof(functionRepository));
        }

        /// <summary>
        /// Returns new instance with a variable added
        /// </summary>
        public ExecutionFrame AddVariable(string key, JsonValue value)
        {
            var newVars = new Dictionary<string, JsonValue>(Variables);
            newVars[key] = value;
            
            return new ExecutionFrame(
                FunctionRepository,
                newVars
            );
        }
    }
}