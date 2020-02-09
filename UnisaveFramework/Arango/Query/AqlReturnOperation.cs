using System.Collections.Generic;
using System.Linq;
using LightJson;
using Unisave.Arango.Database;
using Unisave.Arango.Expressions;

namespace Unisave.Arango.Query
{
    public class AqlReturnOperation : AqlOperation
    {
        public override AqlOperationType OperationType
            => AqlOperationType.Return;
        
        /// <summary>
        /// Expression that defines what gets returned
        /// </summary>
        public AqlExpression Expression { get; }

        public AqlReturnOperation(AqlExpression expression)
        {
            Expression = expression;
        }

        public override string ToAql()
        {
            return "RETURN " + Expression.ToAql();
        }

        /// <summary>
        /// Evaluates the return statement in the context of a frame stream
        /// </summary>
        public IEnumerable<JsonValue> EvaluateInFrameStream(
            IEnumerable<ExecutionFrame> stream
        )
        {
            return stream.Select(f => Expression.EvaluateInFrame(f));
        }
    }
}