using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LightJson;
using LightJson.Serialization;
using Unisave.Arango.Execution;

namespace Unisave.Arango.Expressions
{
    public class AqlJsonObjectExpression : AqlExpression
    {
        public override AqlExpressionType ExpressionType
            => AqlExpressionType.JsonObject;

        public override ReadOnlyCollection<string> Parameters => parameters; 
        
        private ReadOnlyCollection<string> parameters
            = new ReadOnlyCollection<string>(new List<string>());

        /// <summary>
        /// Items of the JSON object
        /// </summary>
        public Dictionary<string, AqlExpression> Items { get; }
            = new Dictionary<string, AqlExpression>();

        public AqlJsonObjectExpression(JsonObject nonParametrizedPart)
        {
            AddAll(nonParametrizedPart);
        }

        /// <summary>
        /// See the only usage in ExpressionConverter to understand
        /// </summary>
        public AqlJsonObjectExpression CallAddViaArgs(
            List<AqlExpression> arguments
        )
        {
            if (arguments.Count != 1 && arguments.Count != 2)
                throw new ArgumentException("Unexpected number of arguments");
            
            if (arguments[0].ExpressionType != AqlExpressionType.Constant)
                throw new AqlParsingException(
                    "JSON object key cannot be an expression"
                );

            JsonValue keyValue = ((AqlConstantExpression) arguments[0]).Value;

            if (!keyValue.IsString || keyValue.IsNull)
                throw new AqlParsingException(
                    "JSON object key has to be a non-null string"
                );

            string key = keyValue.AsString;
            
            AqlExpression value = new AqlConstantExpression(JsonValue.Null);

            if (arguments.Count == 2)
                value = arguments[1];
            
            return Add(key, value);
        }

        public AqlJsonObjectExpression Add(string key, AqlExpression expression)
        {
            if (expression.Parameters.Count > 0)
            {
                parameters = new ReadOnlyCollection<string>(
                    parameters.Union(expression.Parameters).ToList()
                );
            }
            
            Items.Add(key, expression);
            return this;
        }

        public AqlJsonObjectExpression Add(string key, JsonValue value)
        {
            return Add(key, new AqlConstantExpression(value));
        }
        
        public AqlJsonObjectExpression AddAll(JsonObject obj)
        {
            foreach (var pair in obj)
                Add(pair.Key, pair.Value);
            
            return this;
        }
        
        public override string ToAql()
        {
            var itemStream = Items.Select(pair =>
                JsonWriter.Serialize(pair.Key) + ": " + pair.Value.ToAql()
            );
            
            return "{" + string.Join(", ", itemStream) + "}";
        }
        
        public override JsonValue Evaluate(
            QueryExecutor executor,
            ExecutionFrame frame
        )
        {
            var result = new JsonObject();

            foreach (var pair in Items)
                result.Add(pair.Key, pair.Value.Evaluate(executor, frame));
            
            return result;
        }
    }
}