using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LightJson;
using Unisave.Arango.Emulation;

namespace Unisave.Arango.Execution
{
    /// <summary>
    /// One frame of execution (holds values of variables)
    /// Once created, it's immutable and any mutations create mutated copies
    /// </summary>
    public class ExecutionFrame
    {
        public ReadOnlyDictionary<string, JsonValue> Variables { get; }

        public ExecutionFrame(
            Dictionary<string, JsonValue> variables = null
        )
        {
            Variables = new ReadOnlyDictionary<string, JsonValue>(
                variables ?? new Dictionary<string, JsonValue>()
            );
        }

        /// <summary>
        /// Returns new instance with a variable added
        /// </summary>
        public ExecutionFrame AddVariable(string key, JsonValue value)
        {
            var newVars = new Dictionary<string, JsonValue>(Variables);
            newVars[key] = value;
            
            return new ExecutionFrame(
                newVars
            );
        }
    }
}