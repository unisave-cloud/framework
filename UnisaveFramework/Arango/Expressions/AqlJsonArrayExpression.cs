using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LightJson;
using Unisave.Arango.Execution;

namespace Unisave.Arango.Expressions
{
    public class AqlJsonArrayExpression : AqlExpression
    {
        public override AqlExpressionType ExpressionType
            => AqlExpressionType.JsonArray;
        
        public override ReadOnlyCollection<string> Parameters => parameters; 
        
        private ReadOnlyCollection<string> parameters
            = new ReadOnlyCollection<string>(new List<string>());
        
        /// <summary>
        /// Items of the JSON array
        /// </summary>
        public List<AqlExpression> Items { get; }
            = new List<AqlExpression>();
        
        public AqlJsonArrayExpression() { }
        
        public AqlJsonArrayExpression(JsonArray nonParametrizedPart)
        {
            AddAll(nonParametrizedPart);
        }
        
        /// <summary>
        /// See the only usage in ExpressionConverter to understand
        /// </summary>
        public AqlJsonArrayExpression CallAddViaArgs(
            List<AqlExpression> arguments
        )
        {
            if (arguments.Count != 1)
                throw new ArgumentException("Unexpected number of arguments");
            
            return Add(arguments[0]);
        }
        
        public AqlJsonArrayExpression Add(AqlExpression expression)
        {
            if (expression.Parameters.Count > 0)
            {
                parameters = new ReadOnlyCollection<string>(
                    parameters.Union(expression.Parameters).ToList()
                );
            }
            
            Items.Add(expression);
            return this;
        }
        
        public AqlJsonArrayExpression Add(JsonValue value)
        {
            return Add(new AqlConstantExpression(value));
        }
        
        public AqlJsonArrayExpression AddAll(JsonArray arr)
        {
            foreach (var item in arr)
                Add(item);
            
            return this;
        }
        
        public override string ToAql()
        {
            var itemStream = Items.Select(item => item.ToAql());
            return "[" + string.Join(", ", itemStream) + "]";
        }

        public override JsonValue Evaluate(
            QueryExecutor executor,
            ExecutionFrame frame
        )
        {
            var result = new JsonArray();

            foreach (var item in Items)
                result.Add(item.Evaluate(executor, frame));
            
            return result;
        }
    }
}