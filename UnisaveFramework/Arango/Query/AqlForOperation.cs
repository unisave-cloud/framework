using System.Collections.Generic;
using System.Linq;
using LightJson;
using Unisave.Arango.Execution;
using Unisave.Arango.Expressions;

namespace Unisave.Arango.Query
{
    public class AqlForOperation : AqlOperation
    {
        public override AqlOperationType OperationType
            => AqlOperationType.For;
        
        public string VariableName { get; }
        public AqlExpression CollectionExpression { get; }

        public AqlForOperation(
            string variableName,
            AqlExpression collectionExpression
        )
        {
            VariableName = variableName;
            CollectionExpression = collectionExpression;
        }
        
        public override string ToAql()
        {
            return "FOR " + VariableName + " IN " + CollectionExpression.ToAql();
        }

        /// <summary>
        /// Applies the FOR operation to a frame stream, expanding it
        /// </summary>
        public IEnumerable<ExecutionFrame> ApplyToFrameStream(
            IEnumerable<ExecutionFrame> frameStream
        )
        {
            return frameStream.SelectMany(MultiplyFrameByCollection);
        }

        private IEnumerable<ExecutionFrame> MultiplyFrameByCollection(
            ExecutionFrame frame
        )
        {
            IEnumerable<JsonValue> collection = CollectionExpression
                .EvaluateInFrame(frame)
                .AsJsonArray;

            return collection.Select(
                item => frame.AddVariable(VariableName, item)
            );
        }
    }
}