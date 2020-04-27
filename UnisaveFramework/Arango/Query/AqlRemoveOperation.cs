using System.Collections.Generic;
using LightJson;
using Unisave.Arango.Execution;
using Unisave.Arango.Expressions;

namespace Unisave.Arango.Query
{
    public class AqlRemoveOperation : AqlOperation
    {
        public override AqlOperationType OperationType
            => AqlOperationType.Remove;
        
        /// <summary>
        /// Expression that defines the key
        /// </summary>
        public AqlExpression KeyExpression { get; }
        
        /// <summary>
        /// Name of the collection to remove from
        /// </summary>
        public string CollectionName { get; }
        
        /// <summary>
        /// Options object used for the removal
        /// </summary>
        public JsonObject Options { get; }
        
        public AqlRemoveOperation(
            AqlExpression keyExpression,
            string collectionName,
            JsonObject options = null
        )
        {
            ArangoUtils.ValidateCollectionName(collectionName);
            
            KeyExpression = keyExpression;
            CollectionName = collectionName;
            Options = options ?? new JsonObject();
        }
        
        public override string ToAql()
        {
            string aql = "REMOVE " + KeyExpression.ToAql();

            aql += " IN " + CollectionName;
            
            if (Options.Count > 0)
                aql += " OPTIONS " + Options.ToString();

            return aql;
        }
        
        /// <summary>
        /// Performs the REMOVE operation on a frame stream
        /// </summary>
        public IEnumerable<ExecutionFrame> ApplyToFrameStream(
            QueryExecutor executor,
            IEnumerable<ExecutionFrame> frameStream
        )
        {
            foreach (ExecutionFrame frame in frameStream)
            {
                JsonValue keyResult = KeyExpression.Evaluate(executor, frame);
                
                // get key & ref
                string key;
                string rev;
                if (keyResult.IsJsonObject)
                {
                    key = keyResult["_key"].AsString;
                    rev = keyResult["_rev"].AsString;
                }
                else
                {
                    key = keyResult.AsString;
                    rev = null;
                }

                // OLD
                JsonObject oldDocument = executor.DataSource.GetDocument(
                    CollectionName,
                    key
                );
                
                // remove
                executor.DataSource.RemoveDocument(
                    CollectionName,
                    key,
                    rev,
                    Options
                );

                // update frame
                yield return frame
                    .AddVariable("OLD", oldDocument);
            }
        }
    }
}