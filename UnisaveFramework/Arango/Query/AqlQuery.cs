using System;
using System.Linq.Expressions;
using LightJson;

namespace Unisave.Arango.Query
{
    /// <summary>
    /// Represents an AQL query
    /// </summary>
    public class AqlQuery
    {
        // contains a list of "operations"

        public AqlQuery Return(Expression<Func<JsonValue>> e)
        {
            // TODO: add the query to the list, check if not already present
            return this;
        }
        
        public AqlQuery Return(Expression<Func<JsonValue, JsonValue>> e)
        {
            // TODO: add the query to the list, check if not already present
            return this;
        }
    }
}