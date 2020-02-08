using System;
using System.Collections.Generic;
using System.Linq;
using LightJson;
using LightJson.Serialization;

namespace Unisave.Arango.Expressions
{
    public class AqlJsonObjectExpression : AqlExpression
    {
        public override AqlExpressionType ExpressionType
            => AqlExpressionType.JsonObject;

        public override bool HasParameters
            => true; // this instance wouldn't exist otherwise

        /// <summary>
        /// Items of the JSON object
        /// </summary>
        public Dictionary<string, AqlExpression> Items { get; }
            = new Dictionary<string, AqlExpression>();

        public AqlJsonObjectExpression(JsonObject nonParametrizedPart)
        {
            AddAll(nonParametrizedPart);
        }

        public AqlJsonObjectExpression Add(List<AqlExpression> arguments)
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
    }
}