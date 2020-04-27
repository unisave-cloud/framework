using System.Collections.Generic;
using System.Linq.Expressions;
using LightJson;
using Unisave.Arango.Execution;
using Unisave.Arango.Expressions;

namespace Unisave.Arango.Query
{
    public class AqlReplaceOperation : AqlOperation
    {
        public override AqlOperationType OperationType
            => AqlOperationType.Replace;
        
        /// <summary>
        /// Expression that defines the key
        /// </summary>
        public AqlExpression KeyExpression { get; }
        
        /// <summary>
        /// Expression that defines what gets inserted instead of the document
        /// </summary>
        public AqlExpression WithExpression { get; }
        
        /// <summary>
        /// Name of the collection to insert into
        /// </summary>
        public string CollectionName { get; }
        
        /// <summary>
        /// Options object used for the replacement
        /// </summary>
        public JsonObject Options { get; }
        
        public AqlReplaceOperation(
            AqlExpression keyExpression,
            AqlExpression withExpression,
            string collectionName,
            JsonObject options = null
        )
        {
            ArangoUtils.ValidateCollectionName(collectionName);
            
            KeyExpression = keyExpression;
            WithExpression = withExpression;
            CollectionName = collectionName;
            Options = options ?? new JsonObject();
        }
        
        public override string ToAql()
        {
            string aql = "REPLACE " + KeyExpression.ToAql();
            
            if (WithExpression != null)
                aql += " WITH " + WithExpression.ToAql();

            aql += " IN " + CollectionName;
            
            if (Options.Count > 0)
                aql += " OPTIONS " + Options.ToString();

            return aql;
        }
        
        /// <summary>
        /// Performs the REPLACE operation on a frame stream
        /// </summary>
        public IEnumerable<ExecutionFrame> ApplyToFrameStream(
            QueryExecutor executor,
            IEnumerable<ExecutionFrame> frameStream
        )
        {
            foreach (ExecutionFrame frame in frameStream)
            {
                // get key
                JsonValue keyResult = KeyExpression.Evaluate(executor, frame);
                string key = keyResult.AsString;
                if (keyResult.IsJsonObject)
                    key = keyResult["_key"].AsString;

                // get document
                JsonObject document = keyResult;
                if (WithExpression != null)
                    document = WithExpression.Evaluate(executor, frame);

                // OLD
                JsonObject oldDocument = executor.DataSource.GetDocument(
                    CollectionName,
                    key
                );
                
                // replace & NEW
                JsonObject newDocument = executor.DataSource.ReplaceDocument(
                    CollectionName,
                    key,
                    document,
                    Options
                );

                // update frame
                yield return frame
                    .AddVariable("OLD", oldDocument)
                    .AddVariable("NEW", newDocument);
            }
        }
    }
}