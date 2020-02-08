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
        /// Does this expression branch contain any parameters
        /// </summary>
        public abstract bool HasParameters { get; }
        
        /// <summary>
        /// Converts the expression to AQL string
        /// </summary>
        public abstract string ToAql();
    }
}