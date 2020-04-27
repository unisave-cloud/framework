using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using LightJson;
using Unisave.Arango.Execution;
using Unisave.Entities;
using Unisave.Serialization;

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
            return new AqlConstantExpression(Serializer.ToJson(value));
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