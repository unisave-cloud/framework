using System.Collections.Generic;
using LightJson;
using Unisave.Arango.Execution;
using Unisave.Arango.Expressions;

namespace Unisave.Arango.Query
{
    public class AqlInsertOperation : AqlOperation
    {
        public override AqlOperationType OperationType
            => AqlOperationType.Insert;

        /// <summary>
        /// Expression that defines what gets inserted
        /// </summary>
        public AqlExpression Expression { get; }
        
        /// <summary>
        /// Name of the collection to insert into
        /// </summary>
        public string CollectionName { get; }
        
        /// <summary>
        /// Options object used for the insertion
        /// </summary>
        public JsonObject Options { get; }
        
        public AqlInsertOperation(
            AqlExpression expression,
            string collectionName,
            JsonObject options = null
        )
        {
            ArangoUtils.ValidateCollectionName(collectionName);
            
            Expression = expression;
            CollectionName = collectionName;
            Options = options ?? new JsonObject();
        }
        
        public override string ToAql()
        {
            string insertPart = "INSERT " + Expression.ToAql() +
                                " INTO " + CollectionName;

            if (Options.Count == 0)
                return insertPart;

            return insertPart + " OPTIONS " + Options.ToString();
        }
        
        /// <summary>
        /// Performs the INSERT operation on a frame stream
        /// </summary>
        public IEnumerable<ExecutionFrame> ApplyToFrameStream(
            QueryExecutor executor,
            IEnumerable<ExecutionFrame> frameStream
        )
        {
            foreach (ExecutionFrame frame in frameStream)
            {
                JsonObject document = Expression.Evaluate(executor, frame);

                JsonObject newDocument = executor.DataSource.InsertDocument(
                    CollectionName,
                    document,
                    Options
                );
                
                yield return frame.AddVariable("NEW", newDocument);
            }
        }
    }
}