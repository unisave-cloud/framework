using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using LightJson;
using Unisave.Arango.Expressions;

namespace Unisave.Arango.Query
{
    /// <summary>
    /// Represents an AQL query
    /// </summary>
    public class AqlQuery : IEnumerable<AqlOperation>
    {
        /// <summary>
        /// List of AQL operations, making up the query
        /// </summary>
        protected List<AqlOperation> operations = new List<AqlOperation>();
        
        /// <summary>
        /// Variables that have been defined so far and can be used
        /// </summary>
        protected HashSet<string> variables = new HashSet<string>();

        /// <summary>
        /// Helper for parsing AQL expressions
        /// </summary>
        protected LinqToAqlExpressionParser Parser { get; }
            = new LinqToAqlExpressionParser();
        
        public AqlQuery Return(Expression<Func<JsonValue>> e)
            => AddReturnOperation(e.Body);

        public AqlQuery Return(Expression<Func<JsonValue, JsonValue>> e)
            => AddReturnOperation(e.Body);
        
        public AqlQuery Return(Expression<Func<JsonValue, JsonValue, JsonValue>> e)
            => AddReturnOperation(e.Body);

        public AqlQuery AddReturnOperation(Expression e)
        {
            if (operations.Any(o => o is AqlReturnOperation))
                throw new InvalidQueryException(
                    "Query already contains a RETURN operation."
                );
            
            var parsed = Parser.ParseExpression(e);
            
            ValidateParametersCanBeResolved(parsed.Parameters);
            
            operations.Add(new AqlReturnOperation(parsed));
            
            return this;
        }

        protected void ValidateParametersCanBeResolved(
            ReadOnlyCollection<string> parameters
        )
        {
            foreach (string parameter in parameters)
            {
                if (!variables.Contains(parameter))
                    throw new InvalidQueryException(
                        $"Cannot resolve expression parameter '{parameter}', "
                        + "available variables are: " +
                        string.Join(", ", variables)
                    );
            }
        }

        /// <summary>
        /// Check that the query is valid and can be executed
        /// </summary>
        public void ValidateQuery()
        {
            ValidateHasWorkhorseOperation();
        }

        /// <summary>
        /// Check there is a return or data modification operation somewhere
        /// </summary>
        private void ValidateHasWorkhorseOperation()
        {
            // has RETURN
            if (operations.Any(o => o.GetType() == typeof(AqlReturnOperation)))
                return;

            throw new InvalidQueryException(
                "Query is missing any RETURN or data-modification operations."
            );
        }

        /// <summary>
        /// Renders the query to an AQL string
        /// </summary>
        public string ToAql()
        {
            return string.Join("\n", operations.Select(o => o.ToAql()));
        }

        public IEnumerator<AqlOperation> GetEnumerator()
        {
            return operations.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}