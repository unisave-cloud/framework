using System;
using System.Linq.Expressions;
using LightJson;

namespace Unisave.Arango.Expressions
{
    /// <summary>
    /// Converts LINQ expression trees to AQL expression trees
    /// </summary>
    public class LinqToAqlExpressionParser
    {
        private readonly ExpressionDemungifier demungifier
            = new ExpressionDemungifier();

        private readonly ExpressionConverter converter
            = new ExpressionConverter();
        
        public AqlExpression Parse(
            Expression<Func<JsonValue>> e
        ) => ParseExpression(e.Body);
        
        public AqlExpression Parse(
            Expression<Func<JsonValue, JsonValue>> e
        ) => ParseExpression(e.Body);
        
        public AqlExpression Parse(
            Expression<Func<JsonValue, JsonValue, JsonValue>> e
        ) => ParseExpression(e.Body);
        
        public AqlExpression Parse(
            Expression<Func<JsonValue, JsonValue, JsonValue, JsonValue>> e
        ) => ParseExpression(e.Body);

        public AqlExpression ParseEntity<TEntity>(
            Expression<Func<TEntity, JsonValue>> e
        ) where TEntity : Entity
            => ParseExpression(e.Body);

        /// <summary>
        /// Parse an expression tree directly, without the lambda wrapper
        /// </summary>
        public AqlExpression ParseExpression(Expression expression)
        {
            // simplify parts that have no parameters
            Expression demungified = demungifier.Demungify(expression);

            // convert to AQL expression
            AqlExpression converted = converter.Convert(demungified);
            
            return converted;
        }
    }
}