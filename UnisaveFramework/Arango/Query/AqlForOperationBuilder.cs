using System;
using System.Linq.Expressions;
using LightJson;
using Unisave.Arango.Expressions;

namespace Unisave.Arango.Query
{
    public class AqlForOperationBuilder
    {
        /*
         * Usage ideas:
         * For("u").In("users").Do()
         * For("u").In("users").Options(...).Do()
         * For("u").In(jsonConstantArray).Do()
         * For("v", "e", "p").InTraversal("1..3", "any", "graph")
         */
        
        private readonly Action<AqlForOperationBuilder> doneCallback;
        private readonly AqlQuery query;
        
        /// <summary>
        /// Are we building a traversal expression or just a regular one?
        /// </summary>
        public bool? IsTraversal { get; private set; }
        
        /// <summary>
        /// Collection to be iterated over
        /// </summary>
        public AqlExpression CollectionExpression { get; private set; }
        
        public AqlForOperationBuilder(
            AqlQuery query,
            Action<AqlForOperationBuilder> doneCallback
        )
        {
            this.query = query;
            this.doneCallback = doneCallback;
        }

        #region "Fluent API"
        
        /// <summary>
        /// Iterate over a collection
        /// </summary>
        /// <param name="collectionName">Collection name</param>
        public AqlForOperationBuilder In(string collectionName)
            => In(() => AF.Collection(collectionName));

        public AqlForOperationBuilder In(Expression<Func<JsonArray>> e)
            => InRawExpression(e.Body);
        
        public AqlForOperationBuilder In(Expression<Func<JsonValue, JsonArray>> e)
            => InRawExpression(e.Body);
        
        public AqlForOperationBuilder In(
            Expression<Func<JsonValue, JsonValue, JsonArray>> e
        ) => InRawExpression(e.Body);
        
        #endregion

        public AqlForOperationBuilder InRawExpression(Expression e)
        {
            var parser = new LinqToAqlExpressionParser();
            return InRawExpression(parser.ParseExpression(e));
        }
        
        protected AqlForOperationBuilder InRawExpression(AqlExpression e)
        {
            IsTraversal = false;
            CollectionExpression = e;
            return this;
        }

        /// <summary>
        /// Complete the FOR operation
        /// </summary>
        public AqlQuery Do()
        {
            doneCallback.Invoke(this);
            
            return query;
        }
    }
}