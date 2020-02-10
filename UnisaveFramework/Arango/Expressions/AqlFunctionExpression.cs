using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LightJson;
using Unisave.Arango.Execution;

namespace Unisave.Arango.Expressions
{
    public class AqlFunctionExpression : AqlExpression
    {
        public override AqlExpressionType ExpressionType
            => AqlExpressionType.Function;

        public override ReadOnlyCollection<string> Parameters { get; }
            = new ReadOnlyCollection<string>(new List<string>());
        
        public override bool CanSimplify => false;

        /// <summary>
        /// Name of the AQL function
        /// </summary>
        public string FunctionName { get; }
        
        public List<AqlExpression> Arguments { get; }
        
        public AqlFunctionExpression(
            string functionName,
            List<AqlExpression> arguments
        )
        {
            FunctionName = functionName;
            Arguments = arguments;
        }
        
        public AqlFunctionExpression(
            ArangoFunctionAttribute attribute,
            List<AqlExpression> arguments
        ) : this(attribute.FunctionName, arguments)
        { }
        
        public AqlFunctionExpression(
            string functionName,
            params AqlExpression[] arguments
        ) : this(functionName, arguments.ToList())
        { }
        
        public override string ToAql()
        {
            // handle collection access differently
            if (FunctionName.ToUpper() == "COLLECTION"
                && Arguments.Count == 1
                && Arguments[0] is AqlConstantExpression c)
            {
                return c.Value.AsString;
            }
            
            return FunctionName + "(" +
                string.Join(", ", Arguments.Select(a => a.ToAql())) +
                ")";
        }
        
        public override JsonValue Evaluate(
            QueryExecutor executor,
            ExecutionFrame frame
        )
        {
            JsonValue[] evaluatedArguments = Arguments.Select(
                a => a.Evaluate(executor, frame)
            ).ToArray();

            return executor.FunctionRepository.EvaluateFunction(
                FunctionName,
                evaluatedArguments
            );
        }
    }
}