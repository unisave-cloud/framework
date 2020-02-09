using System.Collections.ObjectModel;
using LightJson;
using Unisave.Arango.Database;

namespace Unisave.Arango.Expressions
{
    /// <summary>
    /// Represents an AQL expression
    /// </summary>
    public abstract class AqlExpression
    {
        /// <summary>
        /// Type of the expression node
        /// </summary>
        public abstract AqlExpressionType ExpressionType { get; }

        /// <summary>
        /// Can this branch be evaluated at definition time?
        /// </summary>
        public virtual bool CanSimplify => Parameters.Count == 0;
        
        /// <summary>
        /// Parameters that this expression branch contains
        /// (and needs in order to be evaluated)
        /// </summary>
        public abstract ReadOnlyCollection<string> Parameters { get; }
        
        /// <summary>
        /// Converts the expression to AQL string
        /// </summary>
        public abstract string ToAql();

        /// <summary>
        /// Evaluates the expression to a concrete value
        /// in a given execution frame context
        /// </summary>
        public abstract JsonValue EvaluateInFrame(ExecutionFrame frame);
    }
}