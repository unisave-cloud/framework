using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LightJson;
using Unisave.Arango.Execution;
using Unisave.Entities;

namespace Unisave.Arango.Expressions
{
    public class AqlConstantExpression : AqlExpression
    {
        public override AqlExpressionType ExpressionType
            => AqlExpressionType.Constant;

        public override ReadOnlyCollection<string> Parameters { get; }
            = new ReadOnlyCollection<string>(new List<string>());

        /// <summary>
        /// The value of the constant
        /// </summary>
        public JsonValue Value { get; }

        public AqlConstantExpression(JsonValue value)
        {
            Value = value;
        }

        public static AqlConstantExpression Create(object value)
        {
            switch (value)
            {
                case null:
                    return new AqlConstantExpression(JsonValue.Null);
                
                case JsonObject v:
                    return new AqlConstantExpression(v);
                
                case JsonArray v:
                    return new AqlConstantExpression(v);
                
                case int v:
                    return new AqlConstantExpression(v);
                
                case long v:
                    return new AqlConstantExpression(v);
                
                case double v:
                    return new AqlConstantExpression(v);
                
                case float v:
                    return new AqlConstantExpression(v);
                
                case bool v:
                    return new AqlConstantExpression(v);
                
                case string v:
                    return new AqlConstantExpression(v);
            }
            
            // try unboxing a boxed, json-valued null
            try
            {
                JsonValue unboxedValue = (JsonValue)value;
                return new AqlConstantExpression(unboxedValue);
            }
            catch (InvalidCastException) {}
            
            // get type of the value
            Type valueType = value.GetType();

            // try handling entity reference
            // (when entity reference is compared to an entity, the
            // entity is automatically converted to an entity reference
            // by the demungifier)
            if (valueType.IsGenericType && valueType.GetGenericTypeDefinition()
                == typeof(EntityReference<>))
            {
                var pi = valueType.GetProperty(
                    nameof(EntityReference<Entity>.TargetId)
                );
                return new AqlConstantExpression(
                    (string)pi.GetValue(value)
                );
            }
            
            throw new AqlParsingException(
                $"Value {value} is not a JSON constant."
            );
        }
        
        public override string ToAql()
        {
            return Value.ToString();
        }
        
        public override JsonValue Evaluate(
            QueryExecutor executor,
            ExecutionFrame frame
        )
        {
            return Value;
        }
    }
}