using System;
using System.Linq.Expressions;
using LightJson;
using Unisave.Arango.Expressions;

namespace Unisave.Arango.Query
{
    public class AqlReplaceOperationBuilder
    {
        private readonly Action<AqlReplaceOperationBuilder> doneCallback;
        private readonly AqlQuery query;
        
        public AqlExpression WithExpression { get; private set; }
        public string CollectionName { get; private set; }
        public JsonObject Options { get; private set; }
        
        public AqlReplaceOperationBuilder(
            AqlQuery query,
            Action<AqlReplaceOperationBuilder> doneCallback
        )
        {
            this.query = query;
            this.doneCallback = doneCallback;
        }
        
        #region "Fluent API"

        public AqlReplaceOperationBuilder With(
            Expression<Func<JsonObject>> e
        ) => WithRawExpression(e.Body);
        
        public AqlReplaceOperationBuilder With(
            Expression<Func<JsonValue, JsonObject>> e
        ) => WithRawExpression(e.Body);
        
        public AqlReplaceOperationBuilder With(
            Expression<Func<JsonValue, JsonValue, JsonObject>> e
        ) => WithRawExpression(e.Body);

        /// <summary>
        /// Collection into which to insert
        /// </summary>
        public AqlQuery In(string collectionName)
        {
            ArangoUtils.ValidateCollectionName(collectionName);
            CollectionName = collectionName;
            
            doneCallback.Invoke(this);
            
            return query;
        }

        /// <summary>
        /// Set an options value
        /// </summary>
        public AqlReplaceOperationBuilder WithOption(
            string option,
            JsonValue value
        )
        {
            if (Options == null)
                Options = new JsonObject();

            Options.Add(option, value);
            
            return this;
        }

        /// <summary>
        /// Suppress errors due to non-existing keys
        /// </summary>
        public AqlReplaceOperationBuilder IgnoreErrors(bool value = true)
            => WithOption("ignoreErrors", value);
        
        /// <summary>
        /// Make sure the data will be written to disk when the query finishes
        /// </summary>
        public AqlReplaceOperationBuilder WaitForSync(bool value = true)
            => WithOption("waitForSync", value);
        
        /// <summary>
        /// Check document revisions when doing the update to detect conflicts
        /// </summary>
        public AqlReplaceOperationBuilder CheckRevs(bool check = true)
            => WithOption("ignoreRevs", !check);
        
        /// <summary>
        /// Ignore document revisions (conflict detection)
        /// </summary>
        public AqlReplaceOperationBuilder IgnoreRevs(bool ignore = true)
            => WithOption("ignoreRevs", ignore);
        
        /// <summary>
        /// Request collection-level exclusive lock on RocksDB engine
        /// </summary>
        public AqlReplaceOperationBuilder Exclusive(bool value = true)
            => WithOption("exclusive", value);

        #endregion
        
        public AqlReplaceOperationBuilder WithRawExpression(Expression e)
        {
            var parser = new LinqToAqlExpressionParser();
            return WithRawExpression(parser.ParseExpression(e));
        }
        
        public AqlReplaceOperationBuilder WithRawExpression(AqlExpression e)
        {
            WithExpression = e;
            return this;
        }
    }
}