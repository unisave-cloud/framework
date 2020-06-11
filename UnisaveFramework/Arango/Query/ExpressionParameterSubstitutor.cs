using System.Linq.Expressions;

namespace Unisave.Arango.Query
{
    /// <summary>
    /// Substitutes name of one parameter,
    /// used in entity queries to change lambda expression parameter to "entity"
    /// </summary>
    public class ExpressionParameterSubstitutor : ExpressionVisitor
    {
        private string sourceParameter;
        private string targetParameter;
        
        public ExpressionParameterSubstitutor(
            string sourceParameter,
            string targetParameter
        )
        {
            this.sourceParameter = sourceParameter;
            this.targetParameter = targetParameter;
        }
        
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return Expression.Parameter(
                node.Type,
                node.Name == sourceParameter ? targetParameter : node.Name
            );
        }
    }
}