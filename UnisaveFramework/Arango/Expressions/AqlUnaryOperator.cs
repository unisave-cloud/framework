using System;
using System.Collections.ObjectModel;
using LightJson;
using Unisave.Arango.Database;

namespace Unisave.Arango.Expressions
{
    public class AqlUnaryOperator : AqlExpression
    {
        public override AqlExpressionType ExpressionType { get; }

        public override bool CanSimplify
            => Operand.CanSimplify;

        public override ReadOnlyCollection<string> Parameters
            => Operand.Parameters;
        
        /// <summary>
        /// The only operand of the unary operation
        /// </summary>
        public AqlExpression Operand { get; }

        public AqlUnaryOperator(AqlExpression operand, AqlExpressionType type)
        {
            if (
                type != AqlExpressionType.UnaryPlus
                && type != AqlExpressionType.UnaryMinus
                && type != AqlExpressionType.Not
            )
                throw new ArgumentException(
                    $"Provided expression type {type} is not an unary operation"
                );
            
            ExpressionType = type;
            Operand = operand;
        }
        
        public override string ToAql()
        {
            switch (ExpressionType)
            {
                case AqlExpressionType.UnaryPlus:
                    return "(+" + Operand.ToAql() + ")";
                
                case AqlExpressionType.UnaryMinus:
                    return "(-" + Operand.ToAql() + ")";
                
                case AqlExpressionType.Not:
                    return "(NOT " + Operand.ToAql() + ")";
            }

            return ExpressionType + " " + Operand.ToAql();
        }
        
        public override JsonValue EvaluateInFrame(ExecutionFrame frame)
        {
            throw new System.NotImplementedException();
        }
    }
}