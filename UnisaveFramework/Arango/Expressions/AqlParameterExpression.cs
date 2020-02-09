using System.Collections.Generic;
using System.Collections.ObjectModel;
using LightJson;
using Unisave.Arango.Execution;

namespace Unisave.Arango.Expressions
{
    public class AqlParameterExpression : AqlExpression
    {
        public override AqlExpressionType ExpressionType
            => AqlExpressionType.Parameter;
        
        public override bool CanSimplify => false;
        
        public override ReadOnlyCollection<string> Parameters
            => new ReadOnlyCollection<string>(new List<string> { Name });
        
        /// <summary>
        /// Name of the parameter
        /// </summary>
        public string Name { get; }

        public AqlParameterExpression(string name)
        {
            // TODO: check parameter name for invalid characters
            
            Name = name;
        }

        public override string ToAql()
        {
            return Name;
        }
        
        public override JsonValue EvaluateInFrame(ExecutionFrame frame)
        {
            if (!frame.Variables.ContainsKey(Name))
                throw new QueryExecutionException(
                    $"Parameter {Name} cannot be found. Available variables: "
                    + string.Join(", ", frame.Variables.Keys)
                );

            return frame.Variables[Name];
        }
    }
}