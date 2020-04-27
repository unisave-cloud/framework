using System.Collections.Generic;
using System.Linq;
using Unisave.Arango.Execution;
using Unisave.Arango.Expressions;

namespace Unisave.Arango.Query
{
    public class AqlFilterOperation : AqlOperation
    {
        public override AqlOperationType OperationType
            => AqlOperationType.Filter;

        /// <summary>
        /// Expression that defines what is kept and what it skipped
        /// </summary>
        public AqlExpression Expression { get; }
        
        public AqlFilterOperation(AqlExpression expression)
        {
            Expression = expression;
        }
        
        public override string ToAql()
        {
            return "FILTER " + Expression.ToAql();
        }
        
        /// <summary>
        /// Filters a frame stream
        /// </summary>
        public IEnumerable<ExecutionFrame> ApplyToFrameStream(
            QueryExecutor executor,
            IEnumerable<ExecutionFrame> frameStream
        )
        {
            return frameStream.Where(
                f => Expression.Evaluate(executor, f)
            );
        }
    }
}