using System.Collections.Generic;
using System.Linq;

namespace Unisave.Arango.Expressions
{
    public class AqlFunctionExpression : AqlExpression
    {
        public override AqlExpressionType ExpressionType
            => AqlExpressionType.Function;

        public override bool HasParameters
            => true; // this AQL function itself is a parameter

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
        
        public override string ToAql()
        {
            return FunctionName + "(" +
                string.Join(", ", Arguments.Select(a => a.ToAql())) +
                ")";
        }
    }
}